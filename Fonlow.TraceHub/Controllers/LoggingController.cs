using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Fonlow.Web.Logging.Controllers
{
    [Authorize(Roles = "API")]
    public class LoggingController : Controller
    {
        // GET: LoggingView
        public ActionResult Index()
        {
            return View();
        }
    }
}