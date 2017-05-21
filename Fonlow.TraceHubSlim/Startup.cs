using Microsoft.Owin;
using Owin;

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
