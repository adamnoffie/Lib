using System;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Ucn.Web
{
    /// <summary>
    /// Stores information about an authenticated user to be 'cached' in the 
    /// Forms Authentication ticket
    /// </summary>
    [DataContract]
    public class UserAuthInfo
    {
        public bool IsAuthentic { get; set; }

        public string Username { get; set; }

        [DataMember(Name="id")]
        public int UserID { get; set; }

        [DataMember(Name="aid")]
        public int AccountID { get; set; }
        
        [DataMember]
        public string[] Roles { get; set; }

        /// <summary>
        /// Create a JSON representation of this class
        /// </summary>
        public string ToJson()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer json = new DataContractJsonSerializer(this.GetType());
                json.WriteObject(ms, this);
                return Encoding.ASCII.GetString(ms.ToArray());
            }
        }

        /// <summary>
        /// Create a UserAuthInfo from a JSON representation
        /// </summary>
        /// <param name="xml">xml representation of this class</param>
        static public UserAuthInfo FromJson(string jsonString)
        {
            using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(jsonString)))
            {
                DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(UserAuthInfo));
                return json.ReadObject(ms) as UserAuthInfo;
            }   
        }
    }


    /// <summary>
    /// Used for keeping track of Application specific user session variables
    /// </summary>
    public static class FormsAuth
    {
        // Forms Authentication ticket version
        // Increment if adding new UserData to the ticket?
        private const int FA_TICKET_VERSION = 2;

        private static UserAuthInfo CurrentUserAuthInfo
        {
            get { return (UserAuthInfo)HttpContext.Current.Items["uai"]; }
        }

        /// <summary>
        /// Create a new Forms Authentication ticket stuffed with data
        /// for the current user, then adds it to the response cookies
        /// </summary>
        /// <param name="username"></param>
        public static void Login(UserAuthInfo userAuthInfo)
        {
            if (userAuthInfo.IsAuthentic)
            {
                string userData = userAuthInfo.ToJson();
                DateTime expirationTime = DateTime.Now.AddMinutes(
                    double.Parse(ConfigurationManager.AppSettings["authCookieTimeout"]));

                // Create a new Forms Authentication ticket stuffed with our user Data
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    FA_TICKET_VERSION, userAuthInfo.Username, 
                    DateTime.Now, expirationTime, false, userData);
                string hash = FormsAuthentication.Encrypt(ticket);
                HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, hash);

                // Add the cookie to the list for outbound response
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// Determines if the Form Authentication Identity is filled
        /// </summary>
        /// <returns></returns>
        public static bool IsLoggedIn()
        {
            return (HttpContext.Current.User != null &&
                HttpContext.Current.User.Identity.IsAuthenticated &&
                HttpContext.Current.User.Identity is FormsIdentity);
        }

        /// <summary>
        /// Url to the login page (includes querytstring ReturnUrl='')
        /// </summary>
        public static string LoginUrl
        {
            get
            {
                HttpContext context = HttpContext.Current;
                string url = FormsAuthentication.LoginUrl;

                if (context != null && context.Request != null &&
                    context.Request.RawUrl != FormsAuthentication.LoginUrl)
                {
                    url += "?ReturnUrl=" + HttpUtility.UrlEncode(context.Request.RawUrl);
                }
                return url;
            }
        }

        /// <summary>
        /// Url to the registration page (includes querystring ReturnUrl='')
        /// </summary>
        public static string RegistrationPageUrl
        {
            get
            {
                HttpContext context = HttpContext.Current;
                string url = Utility.Utility.ResolveUrl("~/application.aspx");

                if (context != null && context.Request != null && context.Request.RawUrl != url)
                    url += "?ReturnUrl=" + HttpUtility.UrlEncode(context.Request.RawUrl);

                return url;
            }
        }

        /// <summary>
        /// Grabs the current userID from the FormsAuthentication ticket UserData
        /// </summary>
        /// <returns></returns>
        public static int UserID
        {
            get
            {
                int userID = -1;
                if (IsLoggedIn())
                {
                    return CurrentUserAuthInfo.UserID;
                }
                return userID;
            }
        }

        /// <summary>
        /// Gets the username of the currently logged in user
        /// </summary>
        public static string Username
        {
            get
            {
                if (IsLoggedIn())
                {
                    return HttpContext.Current.User.Identity.Name;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Returns currently logged in users AccountID
        /// (which is the UserID if user is account owner, or super user
        /// </summary>
        public static int AccountID
        {
            get
            {
                int accountID = -1;
                if (IsLoggedIn())
                {
                    return CurrentUserAuthInfo.AccountID;
                }
                return accountID;
            }
        }

        /// <summary>
        /// Gets the roles that the authenticated user is a member of
        /// </summary>
        public static string[] Roles
        {
            get
            {
                if (IsLoggedIn())
                    return CurrentUserAuthInfo.Roles;
                else
                    return new string[0];
            }
        }

        /// <summary>
        /// Force a logout, redirect to login page, and end current response
        /// </summary>
        public static void Logout()
        {
            FormsAuthentication.SignOut();
            //FormsAuthentication.RedirectToLoginPage();
            if (HttpContext.Current != null)
                HttpContext.Current.Response.Redirect(HttpContext.Current.Request.RawUrl);
            HttpContext.Current.Response.End();
        }
    }
}
