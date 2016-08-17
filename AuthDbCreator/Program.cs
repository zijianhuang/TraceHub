using System;
using System.Linq;

using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Fonlow.TraceHub.Security;

namespace AuthDbCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Create database using databaseInitializer defined in config through Code First ...");

            Database.SetInitializer<ApplicationDbContext>(new DropCreateDatabaseAlways<ApplicationDbContext>());
            using (var context = new ApplicationDbContext())
            {
                context.Database.Initialize(false);
                Console.WriteLine(String.Format("Database is initialized, created,  or altered through connection string: {0}", context.Database.Connection.ConnectionString));
                context.SaveChanges();
            }

            Seeding();
            Console.WriteLine("Default users with roles added. Press Enter to exit...");
            Console.ReadLine();

        }

        static void Seeding()
        {
            using (var context = new ApplicationDbContext())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                if (roleManager.Roles.Count() == 0)
                {
                    Console.WriteLine("Creating roles...");
                    var results = RoleConstants.Names.Select(d =>
                    {
                        var r = roleManager.Create(new IdentityRole(d));
                        System.Diagnostics.Trace.TraceInformation(r.Succeeded ? String.Format("Role {0} created.", d)
                            : String.Join("; ", r.Errors));
                        return r;
                    }
                    ).ToArray();
                }

                Console.WriteLine("Now seed the database using ASP .NET Web security and relevant providers...");
                //This bypasses ApplicationUserManager defined in the Service Portal, and related constraints.
                using (var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context)))
                {
                    userManager.CreateUser("zijian.huang@fonlow.com", "zijian.huang@fonlow.com", "Zzzzzzzz*7", RoleConstants.Admin);//will call context.SaveChangesAsync
                    userManager.CreateUser("API", "zijian.huang@fonlow.com", "Aaaaaaaa*7", RoleConstants.Api);
                }

            }



        }
    }


}
