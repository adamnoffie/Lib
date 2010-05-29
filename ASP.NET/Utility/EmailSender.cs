// ----------------------------------------------------------------------------
// EmailSender.cs
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
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Net.Mail;
using System.Web.Configuration;
using System.Net.Configuration;

namespace Utility
{
    /// <summary>
    /// Utility class for sending emails from .NET applications
    /// </summary>
    public static class EmailSender
    {
        /// <summary>
        /// Send an email using server SMTP settings from Web.config
        /// </summary>
        /// <param name="from">FROM field of email</param>
        /// <param name="to">TO field of email</param>
        /// <param name="replyto">REPLY-TO field of email</param>
        /// <param name="subject">The subject of the email message</param>
        /// <param name="body">The body of the email message</param>
        /// <param name="isHTML">Is the body HTML encoded</param>
        /// <param name="useSSL">use SSL when connecting to SMTP server?</param>
        public static void SendEmail(MailAddress from, MailAddress to, MailAddress replyto,
            string subject, string body, bool isHTML, bool useSSL)
        {
            MailMessage mesg = new MailMessage(from, to);
            mesg.Subject = subject;
            mesg.IsBodyHtml = isHTML;
            mesg.Body = body;
            if (replyto != null)
                mesg.ReplyTo = replyto;

            // send the email
            SmtpClient smtp = new SmtpClient();     // pulls smtp settings from Web.config
            smtp.EnableSsl = useSSL;
            smtp.Send(mesg);
        }

        /// <summary>
        /// Sends an email using the SMTP settings stored in the Web.Config file
        /// </summary>
        /// <param name="from">from address of email</param>
        /// <param name="fromDisplayName">display name of from address</param>
        /// <param name="to">who to send email to - this can be multiple addresses separated by semi-colon (;) or commma (,)</param>
        /// <param name="replyto">The replyto address used, leave as string.Empty to not use a reply-to address</param>
        /// <param name="replytoDisplayName">display name of the replyto address</param>
        /// <param name="subject">subject of the email message</param>
        /// <param name="body">body of the email</param>
        /// <param name="isHTML">is the body HTML? - plain text otherwise</param>
        /// <param name="useSSL">use SSL to connect to the SMTP server?</param>
        public static void SendEmail(string from, string fromDisplayName, string to,
            string replyto, string replytoDisplayName, string subject, string body, bool isHTML, bool useSSL)
        {
            MailAddress fromAddress = new MailAddress(from, fromDisplayName);
            MailAddress replyAddress = string.IsNullOrEmpty(replyto) ?
                null : new MailAddress(replyto, replytoDisplayName);

            // send out an email for each address in the to string
            string[] tos = to.Replace(" ", string.Empty).Split(',', ';');
            foreach (string address in tos)
            {
                MailAddress toAddress = new MailAddress(address);
                SendEmail(fromAddress, toAddress, replyAddress, subject, body, isHTML, useSSL);
            }
        }

        /// <summary>
        /// Returns web.config::system.net/mailSettings/smtp/from or 
        /// web.config::system.net/mailSettings/smtp/network/username if the first is not specified
        /// </summary>
        /// <returns></returns>
        public static string GetEmailAddressFromSmtp()
        {
            // Get the configuration <mailSettings> element
            Configuration config = WebConfigurationManager.OpenWebConfiguration(
                HttpContext.Current.Request.ApplicationPath);
            MailSettingsSectionGroup settings = 
                (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
            
            // return the from email address, or if it is empty, return the network username
            if (!string.IsNullOrEmpty(settings.Smtp.From))
                return settings.Smtp.From;
            else
                return settings.Smtp.Network.UserName;
        }
    }
}
