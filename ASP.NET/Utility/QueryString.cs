// ----------------------------------------------------------------------------
// QueryString.cs
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
using System.Collections.Specialized;
using System.Collections;
using System;

namespace System.Web
{
    /// <summary>
    /// Summary description for QueryString
    /// </summary>
    public class QueryString : NameValueCollection
    {
        private string document;

        public string Document
        {
            get { return document; }
        }

        public QueryString() { }
        public QueryString(NameValueCollection clone) : base(clone) { }

        /// <summary>
        /// Returns the QueryString for the current request
        /// </summary>
        /// <returns>QueryString</returns>
        public static QueryString FromCurrent()
        {
            return FromUrl(HttpContext.Current.Request.Url.AbsoluteUri);
        }

        /// <summary>
        /// Returns the QueryString for a specified url
        /// </summary>
        /// <param name="url">The Url to pull the QueryString from</param>
        /// <returns>QueryString</returns>
        public static QueryString FromUrl(string url)
        {
            string[] parts = url.Split("?".ToCharArray());
            QueryString qs = new QueryString();
            qs.document = parts[0];

            if (parts.Length == 1)
                return qs;

            string[] keys = parts[1].Split("&".ToCharArray());
            foreach (string key in keys)
            {
                string[] part = key.Split("=".ToCharArray());
                if (part.Length == 1)
                    qs.Add(part[0], "");
                else if (part.Length > 1)
                    qs.Add(part[0], part[1]);
            }
            return qs;
        }

        /// <summary>
        /// Remove all of the keys except a given one
        /// </summary>
        /// <param name="except">The key NOT to remove</param>
        public void ClearAllExcept(string except)
        {
            ClearAllExcept(new string[] { except });
        }

        /// <summary>
        /// Remove all of the keys except some given ones
        /// </summary>
        /// <param name="except">The keys NOT to remove</param>
        public void ClearAllExcept(string[] except)
        {
            ArrayList toRemove = new ArrayList();
            foreach (string s in this.AllKeys)
            {
                foreach (string e in except)
                {
                    if (s.ToLower() == e.ToLower())
                        if (!toRemove.Contains(s))
                            toRemove.Add(s);
                }
            }

            foreach (string s in toRemove)
                this.Remove(s);
        }

        /// <summary>
        /// Adds a name value pair to the object
        /// </summary>
        public override void Add(string name, string value)
        {
            if (this[name] != null)
                this[name] = value;
            else
                base.Add(name, value);
        }

        /// <summary>
        /// Convert the QueryString to the query string it represents
        /// </summary>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// Convert the QueryString to the query string it represents
        /// </summary>
        /// <param name="includeUrl">Whether to include the document URL or not</param>    
        public string ToString(bool includeUrl)
        {
            string[] parts = new string[this.Count];
            string[] keys = this.AllKeys;

            for (int i = 0; i < keys.Length; i++)
                parts[i] = keys[i] + "=" + this[keys[i]]; //HttpContext.Current.Server.UrlEncode(this[keys[i]]);

            string url = String.Join("&", parts);
            if (!string.IsNullOrEmpty(url) && !url.StartsWith("?"))
                url = "?" + url;

            if (includeUrl)
                url = this.document + url;
            return url;
        }

        /// <summary>
        /// Grabs a parameter from the HttpContext.Current.Request
        /// </summary>    
        /// <param name="sParam">The name of the parameter</param>
        public static bool GetParameter(string sParam, out string sOut)
        {
            if (HttpContext.Current.Request.QueryString[sParam] != null)
            {
                sOut = HttpContext.Current.Request[sParam].ToString();
                return true;
            }
            sOut = string.Empty;
            return false;
        }

        /// <summary>
        /// Grabs an integer parameter from the HttpContext.Current.Request and returns a 
        /// value indicating whether it existed and was parsed correctly
        /// </summary>
        /// <param name="sParam">The name of the parameter</param>
        public static bool GetIntParameter(string sParam, out int iOut)
        {
            if (HttpContext.Current.Request.QueryString[sParam] != null)
            {
                return int.TryParse(HttpContext.Current.Request[sParam], out iOut);
            }
            iOut = 0;
            return false;
        }


        /// <summary>
        /// Grabs a parameter from the HttpContext.Current.Request
        /// </summary>    
        /// <param name="sParam">The name of the parameter</param>
        /// <returns>The parameter, or string.Empty if none was found for sParam</returns>
        public static string GetParameter(string sParam)
        {
            if (HttpContext.Current.Request.QueryString[sParam] != null)
                return HttpContext.Current.Request[sParam].ToString();
            return string.Empty;
        }

        /// <summary>
        /// Grabs an integer parameter from the HttpContext.Current.Request
        /// </summary>
        /// <param name="sParam">The name of the parameter</param>
        /// <returns>The int parameter, or -1 if there was no such parameter.</returns>
        public static int GetIntParameter(string sParam)
        {
            int iOut = -1;
            if (HttpContext.Current.Request.QueryString[sParam] != null)
            {
                string sOut = HttpContext.Current.Request[sParam].ToString();
                if (!String.IsNullOrEmpty(sOut))
                    int.TryParse(sOut, out iOut);
            }
            return iOut;
        }
    } 
}