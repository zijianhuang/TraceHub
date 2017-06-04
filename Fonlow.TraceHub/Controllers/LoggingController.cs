using System.Web.Mvc;

namespace Fonlow.Web.Logging.Controllers
{
    // [Authorize]
    [Authorize(Roles = "API")]//It is recommended to set a special role authorized to view traces which may contain sensitive info.
    public class LoggingController : Controller
    {
        public ActionResult Index()
        {
            return new FilePathResult("~/Views/logging/index.html", "text/html");
        }
    }
}