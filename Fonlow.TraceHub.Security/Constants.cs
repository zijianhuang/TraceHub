using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fonlow.TraceHub.Security
{
    /// <summary>
    /// Roles used in operation contracts and the RoleProvider in ASP .NET
    /// </summary>
    /// <remarks>string constants preferred rather than enum since RoleId is assigned by the DB.</remarks>
    public static class RoleConstants
    {
        /// <summary>
        /// System administrator
        /// </summary>
        public const string Admin = "Admin";

        public const string Api = "Api";

        /// <summary>
        /// All role constants in one array
        /// </summary>
        public readonly static string[] Names = new string[]
        {
            Admin, Api
        };

    }

}
