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

/// <summary>
/// Database logic for Users in system
/// </summary>
public static class UserController
{
    private const string _db_key = "SITE_DataContext";
    private static SITE_DataContext db
    {
        get
        {
            if (HttpContext.Current.Items[_db_key] == null)
                HttpContext.Current.Items[_db_key] = new SITE_DataContext();
            return (SITE_DataContext)HttpContext.Current.Items[_db_key];
        }
    }

    /// <summary>
    /// {0} - url for validation
    /// </summary>
    private const string _ValidateEmailBodyHtml = @"
<div style='background-color: #FFFF80; padding: 10px 20px 50px'>
    <h1>Welcome to WEBSITE_NAME</h1>    
    <p>
        Thank you for registering at WEBSITE_NAME!<br /><br />
        <strong>Please validate your email address by clicking on the following link.</strong><br /><br />
        <a href='{0}'>{0}</a><br /><br />
        If the link above does not work, please copy the following URL and paste it into your web browser's address bar:<br /><br />
        {0}<br /><br />
        Thank you very much,<br /> 
        WEBSITE_NAME<br /> 
        <a href='http://website.com'>website.com</a>
    </p>
</div>";

    /// <summary>
    /// {0} - username, {1} - password
    /// </summary>
    private const string _ResetPasswordEmailBody = @"
<div style='background-color: #FFFF80; padding: 10px 20px 50px'>
    <h1>WEBSITE_NAME Password Reset</h1>    
    <p>
        Your username is:<br/> 
        <span style='font-family: courier new, courier, arial'>{0}</span><br /><br />
        Your new password:<br />
        <strong style='font-family: courier new, courier, arial'>{1}</strong><br /><br /><br />
        Thank you,<br />
        WEBSITE_NAME<br /> 
        <a href='http://website.com'>website.com</a>
    </p>
</div>";

    /// <summary>
    /// Get all of the users
    /// </summary>
    /// <returns></returns>
    public static IQueryable<User> GetAll()
    {
        return db.Users;
    }

    /// <summary>
    /// Get a user given their username
    /// </summary>
    /// <param name="username">username to lookup</param>
    public static User GetByUsername(string username)
    {
        return (from u in db.Users where u.Username == username select u).SingleOrDefault();
    }

    /// <summary>
    /// Gets a user entity given the userID
    /// </summary>
    /// <param name="userID">The id to fetch</param>
    public static User GetByUserID(int userID)
    {
        return (from u in db.Users where u.UserID == userID select u).SingleOrDefault();
    }

    /// <summary>
    /// Get the userID for the current user, if one is logged in
    /// </summary>
    /// <returns></returns>
    public static int GetCurrentUserID()
    {
        if (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
            return GetUserIDByUsername(HttpContext.Current.User.Identity.Name);
        return 0;
    }

    /// <summary>
    /// Gets a user id given the username
    /// </summary>
    /// <param name="username">username to lookup</param>
    public static int GetUserIDByUsername(string username)
    {
        return (from u in db.Users where u.Username == username select u.UserID).SingleOrDefault();
    }
    
    /// <summary>
    /// Gets the user id of user, given their referral code
    /// </summary>
    /// <param name="referralCode">The referral code of the user</param>
    /// <returns></returns>
    public static int GetUserIDByReferralCode(string referralCode)
    {
        return (from u in db.Users
                where u.ReferralCode == referralCode.ToUpper()
                select u.UserID).SingleOrDefault();
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="usr">The User object with data to create</param>
    /// <param name="clearPassword">The cleartext password for the new user</param>
    /// <returns></returns>
    public static void RegisterNew(User usr, string clearPassword)
    {
        // salt + hash the password for storage using BCrypt library
        usr.PasswordHash = BCrypt.HashPassword(clearPassword, BCrypt.GenerateSalt());
        
        // setup extra columns
        usr.CreatedOn = DateTime.UtcNow;
        usr.IsEmailValidated = false;
        usr.ValidationCode = Guid.NewGuid().ToString().Replace("-", string.Empty);
        usr.ReferralCode = usr.ReferralCode ?? string.Empty;

        // send the validation email
        string validateUrl = Utility.WebUtility.ResolveServerUrl(
            "~/ValidateEmail.aspx?code=" + usr.ValidationCode, false);
        string html = _ValidateEmailBodyHtml.Replace("{0}", validateUrl);
        Utility.EmailSender.SendEmail(Utility.EmailSender.GetSmtpFromEmailAddress(), "WEBSITE_NAME",
            usr.EmailAddress, "", "", "Validate your email address", html, true, false);

        // finally, save the user to db
        db.Users.InsertOnSubmit(usr);
        db.SubmitChanges();
    }

    /// <summary>
    /// Register a new REAL ESTATE agent user
    /// </summary>
    /// <param name="usr">The user object with data to create</param>
    /// <param name="clearPassword">cleartext password for new user</param>
    public static void RegisterNewAgent(User usr, string clearPassword)
    {
        // need the random referral code like "RA-392493", random and not in use!
        Random rgen = new Random();
        bool codeInUse = true; 
        string raCode = string.Empty;
        while (codeInUse)
        {
            raCode = "RA-" + rgen.Next(101501, 999999).ToString();
            codeInUse = (from u in db.Users
                         where u.ReferralCode == raCode
                         select u.UserID).Count() > 0;
        }
        usr.ReferralCode = raCode;

        // save the user to DB using existing logic
        RegisterNew(usr, clearPassword);

        // put the user in the Real Estate Agent role
        Roles.AddUserToRole(usr.Username, "Agent");
    }

    /// <summary>
    /// Attempt to send a password reset to a user, if they provide a validated email address
    /// </summary>
    /// <param name="email">email address of user to attempt reset for</param>
    /// <returns></returns>
    public static bool ResetForgottenPassword(string email)
    {
        try
        {
            User user = (from u in db.Users where u.EmailAddress == email select u).SingleOrDefault();
            if (user != null && user.IsEmailValidated && !user.IsAccountLocked)
            {
                // generate a new password for the user
                string newPassword = Membership.GeneratePassword(10, 0);
                user.PasswordHash = BCrypt.HashPassword(newPassword, BCrypt.GenerateSalt(13));

                // send an email with the new password
                string html = _ResetPasswordEmailBody.Replace("{0}", user.Username);
                html = html.Replace("{1}", newPassword);
                Utility.EmailSender.SendEmail(Utility.EmailSender.GetSmtpFromEmailAddress(), "WEBSITE_NAME",
                    user.EmailAddress, "", "", "Password Reset", html, true, false);

                // save new password hash to db
                db.SubmitChanges();
                return true;
            }
            else
                return false;
        }
        catch (Exception)
        {
            return false;
        }
    }


    /// <summary>
    /// Returns true if a user already exists with the specified username
    /// </summary>
    /// <param name="username">username to test</param>
    /// <returns>bool</returns>
    public static bool IsUsernameInUse(string username)
    {
        var qry = from u in db.Users
                  where u.Username == username
                  select u.UserID;
        return qry.Count() > 0;
    }

    /// <summary>
    /// Returns true if a user already exists with the specified email address
    /// </summary>
    /// <param name="email">email address to test</param>
    /// <returns></returns>
    public static bool IsEmailAddressInUse(string email)
    {
        var qry = from u in db.Users
                  where u.EmailAddress == email
                  select u.UserID;
        return qry.Count() > 0;
    }

    /// <summary>
    /// Returns true if a user exists with a given referral code
    /// </summary>
    /// <param name="referralCode">referral code to look for</param>
    public static bool IsReferralCodeValid(string referralCode)
    {
        var qry = from u in db.Users
                  where u.ReferralCode == referralCode.ToUpper()
                  select u.UserID;
        return qry.Count() > 0;
    }

    /// <summary>
    /// Authenticate a user given their credentials
    /// </summary>
    /// <param name="username">username to authenticate</param>
    /// <param name="password">password the user supplied</param>
    /// <returns>success or failure</returns>
    public static bool Authenticate(string username, string password)
    {
        // test for correct username/password, and account is not locked
        string hash = (from u in db.Users 
                       where u.Username == username && !u.IsAccountLocked
                       select u.PasswordHash).SingleOrDefault();
        if (!string.IsNullOrEmpty(hash))
            return BCrypt.CheckPassword(password, hash);
        else
            return false;
    }

    /// <summary>
    /// Attempt to validate a users email address by a validation code
    /// </summary>
    /// <param name="code">the validation code supplied by user</param>
    /// <returns>success or failure</returns>
    public static bool ValidateEmail(string code)
    {
        User usr;
        usr = (from u in db.Users where u.ValidationCode == code select u).SingleOrDefault();

        if (usr != null)
        {
            usr.IsEmailValidated = true;
            db.SubmitChanges();
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// Save changes made to a user
    /// </summary>
    /// <param name="usr">User object to save changes to</param>
    /// <param name="newPassword">a new password if we are changing it, or string.Empty</param>
    /// <param name="changedEmailAddress">true if the user changed email address, false otherwise</param>
    public static void Save(User usr, string newPassword, bool changedEmailAddress)
    {
        // salt + hash the password for storage using BCrypt library if new password was entered
        if (!string.IsNullOrEmpty(newPassword))
            usr.PasswordHash = BCrypt.HashPassword(newPassword, BCrypt.GenerateSalt());

        // was the email address changed, if so we need to redo email validation
        if (changedEmailAddress)
        {
            usr.IsEmailValidated = false;
            usr.ValidationCode = Guid.NewGuid().ToString().Replace("-", string.Empty);

            // send the validation email
            string validateUrl = Utility.WebUtility.ResolveServerUrl(
                "~/ValidateEmail.aspx?code=" + usr.ValidationCode, false);
            string html = _ValidateEmailBodyHtml.Replace("{0}", validateUrl);
            Utility.EmailSender.SendEmail(Utility.EmailSender.GetSmtpFromEmailAddress(), "WEBSITE_NAME",
                usr.EmailAddress, "", "", "Validate your email address", html, true, false);
        }

        // finally, save the user to db
        db.SubmitChanges();
    }

    /// <summary>
    /// Delete a user from the database
    /// </summary>
    /// <param name="u">User to delete</param>
    public static void Delete(User u)
    {
        // delete all of the data for a user, in necessary order for foreign keys
        db.UserRoleMappings.DeleteAllOnSubmit(u.UserRoleMappings);
        foreach (Property p in u.Properties)
        {
            db.PropertyImages.DeleteAllOnSubmit(p.PropertyImages);
            db.PropertyClassificationMappings.DeleteAllOnSubmit(p.PropertyClassificationMappings);
            db.Properties.DeleteOnSubmit(p);
        }
        db.Users.DeleteOnSubmit(u); // finally, delete user
        db.SubmitChanges();
    }
}
