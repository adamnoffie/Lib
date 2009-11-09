// ----------------------------------------------------------------------------
// GoogleMaps.cs
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
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net;
using System.Web.UI;
using System.Configuration;

namespace GoogleMaps
{
    public interface ISpatialCoordinate
    {
        double Latitude { get; set; }
        double Longitude { get; set; }
    }

    /// <summary>
    /// Represents the result of a call to the Google Geocoder
    /// </summary>
    public struct GeocodeResult : ISpatialCoordinate
    {
        /// <summary>
        /// Create a new result with specified latitude and longitude
        /// </summary>
        public GeocodeResult(double latitude, double longitude)
            : this()
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        /// <summary>
        /// The status message returned by the Geocode request
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Coordinate Latitiude
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Coordinate Longitude
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Accuracy of the coordinate if geocoded by Google
        /// </summary>
        public GoogleAddressAccuracy Accuracy { get; set; }

    }

    /// <summary>
    /// Levels of Accuracy of Geocoded results, with Unknown being the worst
    /// </summary>
    public enum GoogleAddressAccuracy : int
    {
        //0 	 Unknown location. (Since 2.59)
        //1 	Country level accuracy. (Since 2.59)
        //2 	Region (state, province, prefecture, etc.) level accuracy. (Since 2.59)
        //3 	Sub-region (county, municipality, etc.) level accuracy. (Since 2.59)
        //4 	Town (city, village) level accuracy. (Since 2.59)
        //5 	Post code (zip code) level accuracy. (Since 2.59)
        //6 	Street level accuracy. (Since 2.59)
        //7 	Intersection level accuracy. (Since 2.59)
        //8 	Address level accuracy. (Since 2.59)
        //9 	Premise (building name, property name, shopping center, etc.) level accuracy. (Since 2.105)
        Unknown = 0,
        Country = 1,
        Region = 2,
        SubRegion = 3,
        Town = 4,
        PostCode = 5,
        StreetLevel = 6,
        Intersection = 7,
        Address = 8,
        Premise = 9
    }

    public struct GoogleApiStatusCodes
    {

        /// <summary>
        /// No errors occurred; the address was successfully parsed and its geocode has been returned.
        /// </summary>
        public const string SUCCESS = "200";

        /// <summary>
        /// A directions request could not be successfully parsed. For example, the request may have 
        /// been rejected if it contained more than the maximum number of waypoints allowed.
        /// </summary>
        public const string BAD_REQUEST = "400";

        /// <summary>
        /// A geocoding or directions request could not be successfully processed, yet the exact 
        /// reason for the failure is not known.
        /// </summary>
        public const string SERVER_ERROR = "500";

        /// <summary>
        /// The HTTP q parameter was either missing or had no value. For geocoding requests, this means 
        /// that an empty address was specified as input. For directions requests, this means that no 
        /// query was specified in the input.
        /// </summary>
        public const string MISSING_QUERY = "601";
        public const string MISSING_ADDRESS = MISSING_QUERY;

        /// <summary>
        /// No corresponding geographic location could be found for the specified address. This may be 
        /// due to the fact that the address is relatively new, or it may be incorrect.
        /// </summary>
        public const string UNKNOWN_ADDRESS = "602";

        /// <summary>
        /// The geocode for the given address or the route for the given directions query cannot 
        /// be returned due to legal or contractual reasons.
        /// </summary>
        public const string UNAVAILABLE_ADDRESS = "603";

        /// <summary>
        /// The GDirections object could not compute directions between the points mentioned in the query. 
        /// This is usually because there is no route available between the two points, 
        /// or because we do not have data for routing in that region.
        /// </summary>
        public const string UNKNOWN_DIRECTIONS = "604";

        /// <summary>
        /// The given key is either invalid or does not match the domain for which it was given.
        /// </summary>
        public const string BAD_KEY = "610";

        /// <summary>
        /// The given key has gone over the requests limit in the 24 hour period or has submitted too many requests 
        /// in too short a period of time. If you're sending multiple requests in parallel or in a tight loop, 
        /// use a timer or pause in your code to make sure you don't send the requests too quickly. 
        /// </summary>
        public const string TOO_MANY_QUERIES = "620";
    }

    /// <summary>
    /// Google Geocoder wrapper class
    /// </summary>
    public class Geocoder
    {
        private const string _geocodeUri = "http://maps.google.com/maps/geo?q=";
        private const string _outputType = "csv"; // Available options: csv, xml, kml, json

        /// <summary>
        /// Returns a Uri of the Google code Geocoding Uri.
        /// </summary>
        /// <param name="address">The address to get the geocode for.</param>
        /// <returns>A new Uri</returns>
        private static Uri GetGeocodeUri(string address)
        {
            string googleKey = ConfigurationManager.AppSettings["gmapApiKey"].ToString();
            address = HttpUtility.UrlEncode(address);
            return new Uri(String.Format("{0}{1}&output={2}&key={3}", _geocodeUri, address, _outputType, googleKey));
        }

        /// <summary>
        /// Gets a Coordinate from a address.
        /// </summary>
        /// <param name="address">An address.
        ///     <remarks>
        ///         <example>
        ///         3276 Westchester Ave, Bronx, NY 10461
        ///         or 
        ///         New York, NY 
        ///         or
        ///         10461  (just a zipcode)
        ///         </example>
        ///     </remarks>
        /// </param>
        /// <returns>A spatial coordinate that contains the latitude and longitude of the address.</returns>
        public static GeocodeResult GeoCode(string address)
        {
            GeocodeResult result;
            if (!string.IsNullOrEmpty(address))
            {
                WebClient client = new WebClient();
                Uri uri = GetGeocodeUri(address);

                // Breakdown of returned CSV values
                // 0 - the status code
                // 1 - accuracy
                // 2 - latitude
                // 3 - longitude
                string[] geocodeInfo = client.DownloadString(uri).Split(',');
                result = new GeocodeResult(double.Parse(geocodeInfo[2]), double.Parse(geocodeInfo[3]));
                result.Status = geocodeInfo[0];
                result.Accuracy = (GoogleAddressAccuracy)int.Parse(geocodeInfo[1]);

                return result;
            }
            else
            {
                result = new GeocodeResult();
                result.Status = GoogleApiStatusCodes.MISSING_QUERY;
                result.Accuracy = GoogleAddressAccuracy.Unknown;
                return result;
            }

        }

    }

    /// <summary>
    /// Google Maps wrapper class
    /// </summary>
    public class GMaps
    {
        private const string _mapsUri = "http://maps.google.com/maps?q={0}";
        private const string _mapsDirectionsUri = "http://maps.google.com/maps?saddr={0}&daddr={1}";

        /// <summary>
        /// Returns a url that will pull up Google Maps centered on a target address
        /// </summary>
        /// <param name="address">The address to place in the url</param>
        public static string GetMapUrl(string address)
        {
            return string.Format(_mapsUri, HttpUtility.UrlEncode(address));
        }

        /// <summary>
        /// Returns a url that will pull up Google Maps with directions from one address to another
        /// </summary>
        /// <param name="startAddress">The start address to map directions from</param>
        /// <param name="endAddress">the end address to map directions to</param>
        /// <returns></returns>
        public static string GetMapDirectionsUrl(string startAddress, string endAddress)
        {
            return string.Format(_mapsDirectionsUri,
                HttpUtility.UrlEncode(startAddress), HttpUtility.UrlEncode(endAddress));
        }
    }
}
