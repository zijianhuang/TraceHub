using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using System;
using Fonlow.TraceHub.Security;
using Microsoft.Owin.Security.OAuth;

[assembly: OwinStartup(typeof(Fonlow.Web.Logging.Startup))]
namespace Fonlow.Web.Logging
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

        //    var hubConfiguration = new HubConfiguration();
#if DEBUG
            //      hubConfiguration.EnableDetailedErrors = true;
#endif

            app.MapSignalR();

        }
    }
}
