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

            app.MapSignalR();

        }
    }
}
