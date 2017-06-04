using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Json;
using System;
using Fonlow.TraceHub.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;

[assembly: OwinStartup(typeof(Fonlow.Web.Logging.Startup))]
namespace Fonlow.Web.Logging
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            //            var hubConfiguration = new HubConfiguration() { f}
#if DEBUG
            //      hubConfiguration.EnableDetailedErrors = true;
#endif

            var settings = JsonUtility.CreateDefaultSerializerSettings();
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            settings.DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"; //otherwise, the milliseconds might have up to 7 dights.
            var serializer = JsonSerializer.Create(settings);
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);

            app.Map("/signalr", map =>
            {
                map.UseOAuthBearerAuthentication(new Microsoft.Owin.Security.OAuth.OAuthBearerAuthenticationOptions
                {
                    Provider = new QueryStringOAuthBearerProvider(),
                    AccessTokenProvider = new Microsoft.Owin.Security.Infrastructure.AuthenticationTokenProvider()
                    {
                        OnCreate = c =>
                        {
                            c.SetToken(c.SerializeTicket());
                        },
                        OnReceive = c =>
                        {
                            c.DeserializeTicket(c.Token);
                            c.OwinContext.Environment["Properties"] = c.Ticket.Properties;
                        }
                    },
                });

                map.RunSignalR(new HubConfiguration
                {
                    Resolver = GlobalHost.DependencyResolver,
                });
            });
        }
    }

    public class QueryStringOAuthBearerProvider : OAuthBearerAuthenticationProvider
    {
        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            var value = context.Request.Query.Get("token");

            if (!string.IsNullOrEmpty(value))
            {
                context.Token = value;
            }

            return Task.FromResult<object>(null);
        }
    }
}
