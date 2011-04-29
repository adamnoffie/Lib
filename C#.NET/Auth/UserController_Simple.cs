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
using Data;

/// <summary>
/// Database logic for Users in system
/// </summary>
public static class UserController
{
    private static LvrDataContext db { get { return DataContext.Current; } }

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
        usr.DateCreated = DateTime.UtcNow;
        
        // finally, save the user to db
        db.Users.InsertOnSubmit(usr);
        db.SubmitChanges();
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
    /// Authenticate a user given their credentials
    /// </summary>
    /// <param name="username">username to authenticate</param>
    /// <param name="password">password the user supplied</param>
    /// <returns>success or failure</returns>
    public static bool Authenticate(string username, string password)
    {
        // test for correct username/password, and account is not locked
        string hash = (from u in db.Users 
                       where u.Username == username
                       select u.PasswordHash).SingleOrDefault();
        if (!string.IsNullOrEmpty(hash))
            return BCrypt.CheckPassword(password, hash);
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

        // TODO: delete other necessary foreign key table entries here

        db.Users.DeleteOnSubmit(u); // finally, delete user
        db.SubmitChanges();
    }
}
