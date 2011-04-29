// ----------------------------------------------------------------------------
// YahooMaps.cs
// Copyright (c) 2009 Adam Nofsinger <adam.nofsinger@gmail.com>
//
// Permission to use, copy, modify, and distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
// ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
// OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
// ----------------------------------------------------------------------------
using System;
using System.Xml;
using System.Configuration;
using System.Web;
using System.Net;
using System.Xml.XPath;
using System.IO;
using System.Text.RegularExpressions;


namespace YahooMaps
{
    /// <summary>
    /// Represents the result of a call to the Yahoo Geocoder
    /// </summary>
    /// <remarks>
    /// Author: Adam Nofsinger (http://adamnoffie.blogspot.com)
    /// </remarks>
    public struct GeocodeResult
    {
        /// <summary>
        /// Create a new result with specified latitude and longitude
        /// </summary>
        public GeocodeResult(double latitude, double longitude) : this()
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// Was the result an error message
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// The error message returend, if there IsError
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Coordinate Latitiude
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Coordinate Longitude
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Precision of the geocode
        /// </summary>
        public YahooAddressPrecision Precision { get; set; }

        /// <summary>
        /// Is there a warning message with the result
        /// </summary>
        public bool IsWarning { get; set; }

        /// <summary>
        /// The warning message returned with results, if there IsWarning
        /// </summary>
        public string WarningMessage { get; set; }

    }

    /// <summary>
    /// From Yahoo: The precision of the address used for geocoding, from specific street address 
    /// all the way up to country, depending on the precision of the address that could be extracted.    /// 
    /// </summary>
    public enum YahooAddressPrecision : int
    {
        Country = 0,
        State = 1,
        City = 2,
        Zip = 3,
        ZipPlus2 = 4,
        ZipPlus4 = 5,
        Street = 6,
        Address = 7
    }

    /// <summary>
    /// Standard Eror messages returned by Yahoo
    /// </summary>
    public struct YahooErrorMessages
    {
        /// <summary>
        /// Too many requests from one IP address in a 24 hour period.
        /// </summary>
        public const string TOO_MANY_REQUESTS = "limit exceeded";
    }

    /// <summary>
    /// Exception to throw if we hit the wall for number of requests you can make to yahoo in one day
    /// </summary>
    [global::System.Serializable]
    public class LimitReachedException : Exception
    {
        public LimitReachedException() { }
        public LimitReachedException(string message) : base(message) { }
    }
		
    /// <summary>
    /// Interfaces w/ Yahoo! Mapping GeoCoding web serice.
    /// </summary>
    public static class Geocoder
    {
        private const string _geocodeUrl = "http://local.yahooapis.com/MapsService/V1/geocode?appid=";

        /// <summary>
        /// Returns a Uri of the Yahoo Geocoding Uri.
        /// </summary>
        /// <param name="address">The address to get the geocode for.</param>
        /// <returns>A new Uri</returns>
        private static Uri GetGeocodeUri(string address)
        {
            string yahooKey = ConfigurationManager.AppSettings["yahooApiKey"].ToString();
            address = HttpUtility.UrlEncode(address);
            return new Uri(String.Format("{0}{1}&location={2}", _geocodeUrl, yahooKey, address));
        }

        /// <summary>
        /// Takes the string returned by Yahoo API for precision and converts to Enum
        /// </summary>
        /// <param name="precision">The precision string representation</param>
        /// <returns>YahooAddressPrecision enum value</returns>
        private static YahooAddressPrecision GetPrecisionEnumValue(string precision)
        {
            switch (precision)
            {   
                case "address": return YahooAddressPrecision.Address;
                case "street": return YahooAddressPrecision.Street;
                case "zip+4": return YahooAddressPrecision.ZipPlus4;
                case "zip+2": return YahooAddressPrecision.ZipPlus2;
                case "zip": return YahooAddressPrecision.Zip;
                case "city": return YahooAddressPrecision.City;
                case "state": return YahooAddressPrecision.State;
                default: return YahooAddressPrecision.Country; //"country"
            }
        }
        
        /// <summary>
        /// Geocode (get lat/lon) for an Address using Yahoo Geocode API
        /// </summary>
        /// <param name="address">the address to geocode</param>
        /// <returns></returns>
        public static GeocodeResult GeoCode(string address)
        {
            GeocodeResult result = new GeocodeResult();
            // Build URL request to be sent to Yahoo!
            Uri url = GetGeocodeUri(address);

            // make request, and create XPathDocument with results
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    XPathDocument xPathDoc = new XPathDocument(response.GetResponseStream());
                    XPathNavigator nav = xPathDoc.CreateNavigator();
                    XmlNamespaceManager ns = new XmlNamespaceManager(nav.NameTable);
                    
                    ns.AddNamespace("yahoo", "urn:yahoo:maps");
                    result.Latitude = double.Parse(nav.SelectSingleNode("//yahoo:Latitude", ns).InnerXml);
                    result.Longitude = double.Parse(nav.SelectSingleNode("//yahoo:Longitude", ns).InnerXml);
                    result.Precision = GetPrecisionEnumValue(
                        nav.SelectSingleNode("//yahoo:Result/@precision", ns).InnerXml);

                    // get the warning if one exists
                    XPathNavigator warningNode = nav.SelectSingleNode("//yahoo:Result/@warning", ns);
                    if (warningNode != null && !string.IsNullOrEmpty(warningNode.InnerXml))
                    {
                        result.IsWarning = true;
                        result.WarningMessage = warningNode.InnerXml;
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (HttpWebResponse errorResponse = ex.Response as HttpWebResponse)
                    {
                        // Grab the error message
                        result.IsError = true;

                        XPathDocument xPathDoc = new XPathDocument(errorResponse.GetResponseStream());
                        XPathNavigator nav = xPathDoc.CreateNavigator();
                        XmlNamespaceManager ns = new XmlNamespaceManager(nav.NameTable);

                        ns.AddNamespace("yahoo", "urn:yahoo:api");
                        result.ErrorMessage = nav.SelectSingleNode("//yahoo:Message", ns).InnerXml;

                        if (result.ErrorMessage == YahooErrorMessages.TOO_MANY_REQUESTS)
                            throw new LimitReachedException();
                    }
                }
            }

            return result;
        }
    }
 }