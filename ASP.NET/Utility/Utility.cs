using System;
using System.Data;
using System.Collections;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;

namespace Utility
{
    /// <summary>
    /// Various functionality common to the Stations application
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Returns "Yes" if true, "No" otherwise
        /// </summary>
        public static string ToYesNoString(this bool boolean)
        {
            return boolean ? "Yes" : "No";
        }

        /// <summary>
        /// Removes any occurences of character(s) from a string
        /// </summary>
        /// <param name="str">string to process</param>
        /// <param name="characters">characters to remove</param>
        public static string RemoveChars(this string str, params char[] characters)
        {
            string removed = str;

            foreach (char c in characters)
                removed = removed.Replace(c.ToString(), string.Empty);

            return removed;
        }

        /// <summary>
        /// Takes a set of keywords seperated by any of the following delimeters and 
        /// returns a search string that can be uses in a CONTAINS or CONTAINSTABLE
        /// predicate in a T-SQL query.
        /// Delimeters: spaces, parenthesis, commas, periods
        /// </summary>
        /// <param name="keywordString">The keyword string.</param>
        /// <param name="useANDs">Use AND to seperate each FT keyword. If false, will use OR</param>
        /// <returns>The FT search usable search string</returns>
        public static string KeywordsToFullText(string keywordString, bool useANDs)
        {
            string[] keywords = keywordString.Split(new char[] { ' ', '(', ')', ',', '.' });

            string qryString = string.Empty;
            if (keywords.Length > 0)
            {
                qryString += "\"*" + keywords[0] + "*\"";                // first word
                for (int i_key = 1; i_key < keywords.Length; i_key++)
                {
                    qryString += (useANDs) ? " AND \"*" : " OR \"*";
                    qryString += keywords[i_key] + "*\"";                // additional
                }
            }

            return qryString;
        }

        /// <summary>
        /// Returns true if the browser is well supported. Currently:
        ///     Firefox 2+, Internet Explorer 7+, Opera 9+
        /// </summary>
        /// <returns>bool</returns>
        public static bool TestBrowser()
        {
            string browser = HttpContext.Current.Request.Browser.Browser;
            int majVersion = HttpContext.Current.Request.Browser.MajorVersion;
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
        public static void ForceSSL()
        {
            // TODO: Re-enable this method once we can afford SSL again!

            //HttpRequest request = HttpContext.Current.Request;

            //if (!request.IsSecureConnection)
            //{
            //    string sslPort = ConfigurationManager.AppSettings["sslPort"];
            //    UriBuilder newURL = new UriBuilder(request.Url);

            //    newURL.Scheme = "https://";
            //    if (!string.IsNullOrEmpty(sslPort))
            //        newURL.Port = int.Parse(sslPort);
            //    else
            //        newURL.Port = 443;

            //    //Force into secure channel
            //    HttpContext.Current.Response.Redirect(newURL.ToString());
            //}
        }

        /// <summary>
        /// Returns a site relative HTTP path from a partial path starting out with a ~.
        /// Same syntax that ASP.Net internally supports but this method can be used
        /// outside of the Page framework.
        /// 
        /// Works like Control.ResolveUrl including support for ~ syntax
        /// but returns an absolute URL.
        /// </summary>
        /// <param name="originalUrl">Any Url including those starting with ~</param>
        /// <returns>relative url</returns>
        public static string ResolveUrl(string originalUrl)
        {
            if (originalUrl == null)
                return null;

            // *** Absolute path - just return
            if (originalUrl.IndexOf("://") != -1)
                return originalUrl;

            // *** Fix up image path for ~ root app dir directory
            if (originalUrl.StartsWith("~"))
                return VirtualPathUtility.ToAbsolute(originalUrl);

            return originalUrl;
        }

        /// <summary>
        /// Returns the base path of the server currently running the aplication 
        /// (e.g. for  http://panel.ims3k.com/somefolder/applicationpath/file.aspx?querystring this
        /// will return http://panel.ims3k.com/somefolder )
        /// </summary>
        /// <param name="useSSL">Should base path start with http:// or https:// </param>
        /// <returns>string</returns>
        internal static string GetBasePath(bool useSSL)
        {
            string basePath = useSSL ? "https://" : "http://";
            basePath += HttpContext.Current.Request.Url.Authority +
                HttpContext.Current.Request.ApplicationPath + "/";
            return basePath;
        }


        public static TResult[] ParseTo<TResult>(this string[] values)
        {
            TResult[] temp = new TResult[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                temp[i] = (TResult)Convert.ChangeType(values[i], typeof(TResult));
            }
            return temp;
        }
    }

    /// <summary>
    /// This class is used to make Enum casts prettier
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Enum<T>
    {
        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        public static IList<T> GetValues()
        {
            IList<T> list = new List<T>();
            foreach (object value in Enum.GetValues(typeof(T)))
            {
                list.Add((T)value);
            }
            return list;
        }

        /// <summary>
        /// Returns an array of ListItems for use in a drop down or select box
        /// </summary>
        public static ListItem[] GetListItems()
        {
            List<ListItem> items = new List<ListItem>();
            Type enumType = typeof(T);
            Type underType = Enum.GetUnderlyingType(enumType);
            foreach (object value in Enum.GetValues(enumType))
            {
                ListItem li = new ListItem(value.ToString(), 
                    Convert.ChangeType(value, underType).ToString());
                items.Add(li);
            }
            return items.ToArray();
        }
    }   
}