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


namespace Auth
{
    /// <summary>
    /// Summary description for AuthRoleProvider
    /// </summary>
    public class AuthRoleProvider : RoleProvider
    {
        private AuthDataContext db;

        public AuthRoleProvider()
        {
            // Change this if you port this provider to another application
            db = new AuthDataContext(ConfigurationManager.ConnectionStrings[1].ConnectionString);
        }

        /// <summary>
        /// Adds the specified users to the specified roles
        /// </summary>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            foreach (string uname in usernames)
            {
                int userID = (from u in db.Users where u.Username == uname select u.UserID).Single();
                foreach (string role in roleNames)
                {
                    UserRoleMapping mapping = new UserRoleMapping();
                    mapping.UserID = userID;
                    mapping.UserRoleID =
                        (from r in db.UserRoles where r.RoleName == role select r.RoleID).Single();
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
            var qry = from u in db.Users
                      where u.Username == username
                      join m in db.UserRoleMappings on u.UserID equals m.UserID
                      join r in db.UserRoles on m.UserRoleID equals r.RoleID
                      select r.RoleName;

            return qry.ToArray();
        }

        /// <summary>
        /// Determine if a user is in a given role
        /// </summary>
        /// <param name="username">The user to check</param>
        /// <param name="roleName">The role to check</param>
        public override bool IsUserInRole(string username, string roleName)
        {
            return GetRolesForUser(username).Contains(roleName);
        }


        /// <summary>
        /// I have not yet run into an application that uses the RolesProvider in a way
        /// that the following methods are actually called!
        /// </summary>
        #region Useless RoleProvider overrides

        public override string ApplicationName
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        #endregion Useless
    }
    
}