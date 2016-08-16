using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNet.Identity;

namespace Fonlow.TraceHub.Security
{
    public static class UserManagerExtensions
    {
        /// <summary>
        /// Create user with single role
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="roleName"></param>
        /// <returns>User Id, or null if there's error</returns>
        public static string CreateUser(this UserManager<ApplicationUser> userManager, string userName, string email, string password, string roleName, bool throwException = false)
        {
            return userManager.CreateUser(new ApplicationUser() { UserName = userName, Email = email }, password, roleName, throwException);
        }

        public static string CreateUser(this UserManager<ApplicationUser> userManager, ApplicationUser user, string password, string roleName, bool throwException = false)
        {
            var r = userManager.Create(user, password);
            if (r.Succeeded)
            {
                if (String.IsNullOrEmpty(roleName))
                {
                    return user.Id; // no role to create
                }

                var rr = userManager.AddToRole(user.Id, roleName);
                if (rr.Succeeded)
                {
                    Trace.TraceInformation("User {0} added to role {1}.", user.UserName, roleName);
                    return user.Id;
                }

                string msg = String.Format("When assigning role {0} to user {1}, errors: {2}", roleName, user.UserName, String.Join(Environment.NewLine, r.Errors));
                if (throwException)
                    throw new System.Security.SecurityException(msg);
                Trace.TraceWarning(msg);
                return null;
            }

            string msg2 = String.Format("When creating user {0}, errors: {1}", user.UserName, String.Join(Environment.NewLine, r.Errors));
            if (throwException)
                throw new System.Security.SecurityException(msg2);

            Trace.TraceWarning(msg2);
            return null;
        }
    }

}
