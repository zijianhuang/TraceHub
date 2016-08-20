using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using System;

[assembly: OwinStartup(typeof(Fonlow.Web.Logging.Startup))]
namespace Fonlow.Web.Logging
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
