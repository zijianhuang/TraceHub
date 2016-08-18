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

            app.UseOAuthAuthorizationServer(OAuthOptions);

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions
            {
                Provider = new ApplicationOAuthBearerAuthenticationProvider(),
            });



            var hubConfiguration = new HubConfiguration();
#if DEBUG
            //      hubConfiguration.EnableDetailedErrors = true;
#endif

            //app.Map("/signalr", map => map.UseOAuthBearerAuthentication(new Microsoft.Owin.Security.OAuth.OAuthBearerAuthenticationOptions()
            //{
            //    Provider = new Fonlow.TraceHub.Security.ApplicationOAuthBearerAuthenticationProvider(),
            //}));

            //GlobalHost.Configuration.DefaultMessageBufferSize = 5000;
            app.MapSignalR();

        }
    }
}
