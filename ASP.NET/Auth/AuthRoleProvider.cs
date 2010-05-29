// ----------------------------------------------------------------------------
// AuthRoleProvider.cs
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
using Data;


namespace Auth
{
    /// <summary>
    /// Summary description for AuthRoleProvider
    /// </summary>
    public class AuthRoleProvider : RoleProvider
    {
        public static ExtremeCageFightingDataContext db { get { return DataContext.Current; } }

        /// <summary>
        /// Adds the specified users to the specified roles
        /// </summary>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            // foreach user
            foreach (string uname in usernames)
            {
                int userID = (from u in db.Users where u.Username == uname select u.UserID).Single();
                // foreach role the user is being added to
                foreach (string role in roleNames)
                {
                    UserRoleMapping mapping = new UserRoleMapping();
                    mapping.UserID = userID;
                    mapping.UserRoleID =
                        (from r in db.UserRoles 
                         where r.RoleName.ToLower() == role.ToLower() 
                         select r.RoleID).Single();
                    db.UserRoleMappings.InsertOnSubmit(mapping);
                }
            }
            db.SubmitChanges();
        }

        /// <summary>
        /// Listing of all role names
        /// </summary>
        /// <returns></returns>
        public override string[] GetAllRoles()
        {
            return (from r in db.UserRoles select r.RoleName).ToArray();
        }

        /// <summary>
        /// Get a list of roles a user is in
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public override string[] GetRolesForUser(string username)
        {
            // TODO: perhaps cache roles in FormsAuthentication ticket cookie
            //if (username == FormsAuth.Username) // grabbing current users roles, which are cached in forms auth ticket
            //{
            //    return FormsAuth.Roles;
            //}        
            return (from u in db.Users
                    where u.Username == username
                    join m in db.UserRoleMappings on u.UserID equals m.UserID
                    join r in db.UserRoles on m.UserRoleID equals r.RoleID
                    select r.RoleName).ToArray();
        }

        /// <summary>
        /// Determine if a user is in a given role
        /// </summary>
        /// <param name="username">The user to check</param>
        /// <param name="roleName">The role to check</param>
        public override bool IsUserInRole(string username, string roleName)
        {
            return GetRolesForUser(username).Count(r => r.ToLower() == roleName.ToLower()) > 0;
        }

        /// <summary>
        /// Return all of the usernames for users that are in a given role
        /// </summary>
        /// <param name="roleName">Name of the role to check</param>
        public override string[] GetUsersInRole(string roleName)
        {
            return (from u in db.Users
                    join m in db.UserRoleMappings on u.UserID equals m.UserID
                    join r in db.UserRoles on m.UserRoleID equals r.RoleID
                    where r.RoleName == roleName
                    select u.Username).ToArray();
        }

        /// <summary>
        /// Remove user(s) from specified role(s)
        /// </summary>
        /// <param name="usernames"></param>
        /// <param name="roleNames"></param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            foreach (string uname in usernames)
            {
                int userID = (from u in db.Users where u.Username == uname select u.UserID).Single();
                db.UserRoleMappings.DeleteAllOnSubmit(
                    from rm in db.UserRoleMappings
                    join r in db.UserRoles on rm.UserRoleID equals r.RoleID
                    where rm.UserID == userID && roleNames.Contains(r.RoleName)
                    select rm);
            }

            db.SubmitChanges();
        }

        /// <summary>
        /// Add a Role
        /// </summary>
        /// <param name="roleName">Name of the Role to add</param>
        public override void CreateRole(string roleName)
        {
            db.UserRoles.InsertOnSubmit(new UserRole() { RoleName = roleName });
            db.SubmitChanges();
        }

        /// <summary>
        /// Remove a user role
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="throwOnPopulatedRole">Should we throw an exception if there are already users in the
        /// Role? If false, the users will be removed from the role, and it will be deleted anyhow.</param>
        /// <returns>true if successfully deleted, false otherwise</returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            UserRole role = db.UserRoles.Where(r => r.RoleName == roleName).SingleOrDefault();

            if (role != null)
            {
                if (!throwOnPopulatedRole)
                    db.UserRoleMappings.DeleteAllOnSubmit(role.UserRoleMappings);
                db.UserRoles.DeleteOnSubmit(role);
                db.SubmitChanges();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Find users in a given role, that match a name
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="usernameToMatch"></param>
        /// <returns></returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return (from u in db.Users
                    join urm in db.UserRoleMappings on u.UserID equals urm.UserID
                    join r in db.UserRoles on urm.UserRoleID equals r.RoleID
                    where u.Username.ToLower().Contains(usernameToMatch.ToLower()) &&
                        r.RoleName.ToLower() == roleName.ToLower()
                    select u.Username).ToArray();
        }

        /// <summary>
        /// Returns true if a role exists, false otherwise
        /// </summary>
        /// <param name="roleName">Name of role to search for</param>
        /// <returns></returns>
        public override bool RoleExists(string roleName)
        {
            return (from r in db.UserRoles 
                    where r.RoleName.ToLower() == roleName.ToLower()
                    select r.RoleID).Count() > 0;
        }

        public override string ApplicationName { get; set; }
    }
    
}