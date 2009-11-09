// ----------------------------------------------------------------------------
// Logging.cs
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
using System.Data;
using System.Configuration;
using System.Web;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Collections.Specialized;

namespace Utility
{
    public static class Logging
    {
        private const string LOGFILE = "errorlog.txt";
        private const string SEPERATOR = "\r\n=====================================================\r\n";

        private static void AppendToTextLog(string textToAppend)
        {
            string file = HttpContext.Current.Request.PhysicalApplicationPath + LOGFILE;
            StreamWriter sw;
            if (!File.Exists(file))
                sw = File.CreateText(file);
            else
                sw = File.AppendText(file);

            sw.Write("   @ " + DateTime.Now.ToString("MM d yyy  hh:mm tt") + SEPERATOR);
            sw.Write(textToAppend);
            sw.Close();
        }

        /// <summary>
        /// Log an error in the application.
        /// Currently write to errorlog.txt in root
        /// Can also send an email to an admin about the error
        /// </summary>
        /// <param name="errorData"></param>
        /// <param name="emailAlert"></param>
        public static void LogError(string errorData, bool emailAlert)
        {
            // trapping exceptions and not rethrowing them so as not to
            // cause an infinite loop (not sure that would happen, seems it might)
            try
            {
                AppendToTextLog(errorData);
            }
            catch (Exception) { }

            string subject = string.Format("[{0}] Application Error!",
                HttpContext.Current.Request.Url.Host);

            if (emailAlert)
                EmailSender.SendEmail(subject, errorData, false);
        }

        /// <summary>
        /// Log an error in the application.
        /// Currently write to errorlog.txt in root
        /// Can also send an email to an admin about the error
        /// </summary>
        /// <param name="heading">String to affix to top of error logging</param>
        /// <param name="emailAlert">If true, send an email to admin</param>
        /// <param name="ex">An exception to auto-build an error logging message from</param>
        public static void LogError(string heading, bool emailAlert, Exception ex)
        {
            HttpRequest curRequest = HttpContext.Current.Request;

            string referer = curRequest.ServerVariables["HTTP_REFERER"] ?? string.Empty;
            string formData = (curRequest.Form ?? new NameValueCollection()).ToString();
            string sQuery = (curRequest.QueryString ?? new NameValueCollection()).ToString();

            StringBuilder sbError = new StringBuilder(heading);
            sbError.AppendLine().Append("SOURCE:      ").Append(ex.Source);
            sbError.AppendLine().Append("MESSAGE:     ").Append(ex.Message);
            sbError.AppendLine().Append("FORM:        ").Append(formData);
            sbError.AppendLine().Append("QUERYSTRING: ").Append(sQuery);
            sbError.AppendLine().Append("TARGETSITE:  ").Append(ex.TargetSite);
            sbError.AppendLine().Append("STACKTRACE:  ").Append(ex.StackTrace);
            sbError.AppendLine().Append("REFERER:     ").Append(referer);
            sbError.AppendLine().Append("USER:        ").Append(
                HttpContext.Current != null ? HttpContext.Current.User.Identity.Name : string.Empty);
            sbError.AppendLine();

            Logging.LogError(sbError.ToString(), emailAlert);
        }
    }
}
