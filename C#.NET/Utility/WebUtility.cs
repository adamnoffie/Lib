// ----------------------------------------------------------------------------
// WebUtility.cs
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
using System.Configuration;
using System.Web;

namespace Utility
{
    /// <summary>
    /// Miscellaneous Utility functions 
    /// </summary>
    public static class WebUtility
    {
        /// <summary>
        /// Returns true if the browser is well supported. Currently:
        ///     Firefox 2+, Internet Explorer 7+, Opera 9+
        /// </summary>
        /// <returns>bool</returns>
        public static bool TestBrowser(HttpContext ctxt)
        {
            string browser = ctxt.Request.Browser.Browser;
            int majVersion = ctxt.Request.Browser.MajorVersion;
            if (browser.Equals("firefox", StringComparison.OrdinalIgnoreCase))
            {
                if (majVersion >= 2)
                    return true;
            }
            else if (browser.Equals("ie", StringComparison.OrdinalIgnoreCase))
            {
                if (majVersion >= 7)
                    return true;
            }
            else if (browser.Equals("opera", StringComparison.OrdinalIgnoreCase))
            {
                if (majVersion >= 9)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Force a redirect to ssl secured version of page (https://) if not already
        /// using ssl -- specifies a port if appSetting 'sslPort' is set in Web.config
        /// </summary>
        public static void ForceSSL(HttpContext ctxt)
        {
            HttpRequest request = ctxt.Request;

            if (!request.IsSecureConnection)
            {
                string sslPort = ConfigurationManager.AppSettings["sslPort"];
                UriBuilder newURL = new UriBuilder(request.Url);

                newURL.Scheme = "https://";
                if (!string.IsNullOrEmpty(sslPort))
                    newURL.Port = int.Parse(sslPort);
                else
                    newURL.Port = 443;

                //Force into secure channel
                ctxt.Response.Redirect(newURL.ToString());
            }
        }

        /// <summary>
        /// Like Control.ResolveUrl or Page.ResolveUrl, but can be used elsewhere.
        /// Returns a relative url
        /// </summary>
        /// <param name="originalUrl">Any Url including those starting with ~</param>
        /// <returns>relative url</returns>
        public static string ResolveUrl(string originalUrl)
        {
            if (!string.IsNullOrEmpty(originalUrl) && '~' == originalUrl[0])
            {
                int index = originalUrl.IndexOf('?');
                string queryString = (-1 == index) ? null : originalUrl.Substring(index);
                if (-1 != index) originalUrl = originalUrl.Substring(0, index);
                originalUrl = VirtualPathUtility.ToAbsolute(originalUrl) + queryString;
            }
            return originalUrl;
        }

        /// <summary>
        /// Like Control.ResolveUrl or Page.Resolveurl but can be used elsewhere.
        /// Returns an abosolute Url
        /// </summary>
        /// <param name="serverUrl">The Url to resolve</param>
        /// <param name="forceHttps">Force a https:// in the url (SSL)</param>
        /// <returns>absolute url</returns>
        public static string ResolveServerUrl(HttpContext ctxt, string serverUrl, bool forceHttps)
        {
            Uri result = ctxt.Request.Url;
            if (!string.IsNullOrEmpty(serverUrl))
            {
                serverUrl = ResolveUrl(serverUrl);
                result = new Uri(result, serverUrl);
            }
            if (forceHttps && !string.Equals(result, Uri.UriSchemeHttps))
            {
                UriBuilder builder = new UriBuilder(result);
                builder.Scheme = Uri.UriSchemeHttps;
                builder.Port = 443;
                result = builder.Uri;
            }

            return result.ToString();
        }


        /// <summary>
        /// Returns the base path of the server currently running the aplication 
        /// (e.g. for  http://panel.ims3k.com/somefolder/applicationpath/file.aspx?querystring this
        /// will return http://panel.ims3k.com/somefolder )
        /// </summary>
        /// <param name="useSSL">Should base path start with http:// or https:// </param>
        /// <returns>string</returns>
        public static string GetBasePath(HttpContext ctxt, bool useSSL)
        {
            string basePath = useSSL ? "https://" : "http://";
            basePath += ctxt.Request.Url.Authority + ctxt.Request.ApplicationPath + "/";
            return basePath;
        }

        /// <summary>
        /// Tell browsers not to cache the current response
        /// </summary>
        /// <param name="context"></param>
        public static void DisableBrowserCaching(HttpContext ctxt)
        {
            ctxt.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            ctxt.Response.Cache.SetValidUntilExpires(false);
            ctxt.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            ctxt.Response.Cache.SetNoStore();
        }
    }
}