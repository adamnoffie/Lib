using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace Utility
{
    /// <summary>
    /// Used for tracing a users path around the site.  Currently is very simple.
    /// Simplay used to mark important "returnable" pages, and then get that
    /// page location on a subsequent page
    /// </summary>
    public static class SiteTracker
    {
        const string siteTrack = "trak";

        /// <summary>
        /// Set the site tracker location to a 'returnable' page.
        /// </summary>
        public static void Mark()
        {
            Mark(HttpContext.Current.Request.Url.ToString());           
        }

        /// <summary>
        /// Set the site tracker location to a 'returnable' page.
        /// </summary>
        /// <param name="address">Address to mark</param>
        public static void Mark(string address)
        {
            HttpContext.Current.Response.Cookies[siteTrack].Value = address; // create or recreates the cookie
        }

        /// <summary>
        /// Gets the last marked location that can be used for "Back" or "Cancel" buttons
        /// from subsequent pages.
        /// </summary>
        public static string GetMark()
        {
            HttpCookie tracker = HttpContext.Current.Request.Cookies[siteTrack];
            
            if (tracker != null) // make sure cookie is not null
                return tracker.Value;

            return null;
        }

        /// <summary>
        /// Gets the last marked location if there is one, otherwise returns the value passed in
        /// </summary>
        /// <param name="defaultBackTo">Value to return if there is no mark</param>
        /// <returns></returns>
        public static string GetMark(string defaultBackTo)
        {
            string backTo = GetMark();

            return (!string.IsNullOrEmpty(backTo)) ? backTo : defaultBackTo;
        }
    }
}
