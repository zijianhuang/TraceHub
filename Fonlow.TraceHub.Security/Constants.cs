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

        /// <summary>
        /// Licensing Manager is authroized to activate and deactivate licences
        /// </summary>
        public const string Manager = "Manager";

        /// <summary>
        /// Support could issue temp licences and view customer info
        /// </summary>
        public const string Staff = "Staff";

        /// <summary>
        /// Shop owner who may access shop info and activate licences assigned to the shop.
        /// </summary>
        public const string Customer = "Customer";

        public const string Api = "Api";

        /// <summary>
        /// All role constants in one array
        /// </summary>
        public readonly static string[] Names = new string[]
        {
            Admin, Manager, Staff,  Customer, Api
        };


        public const string InternalRoles = Admin + "," + Manager + "," + Staff;
        public const string InternalBusinessAdmins = Admin + "," + Manager;


    }

}
