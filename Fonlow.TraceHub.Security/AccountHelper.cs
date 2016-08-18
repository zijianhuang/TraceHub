using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace Fonlow.TraceHub.Security
{
    public static class AccountHelper
    {
        public static bool IsAdmin(System.Security.Principal.IPrincipal user)
        {
            return user.IsInRole(RoleConstants.Admin);
        }

        public static bool IsApiUser(System.Security.Principal.IPrincipal user)
        {
            return user.IsInRole(RoleConstants.Api);
        }

    }
}
