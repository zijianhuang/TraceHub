using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Fonlow.TraceHub.Security;

namespace Fonlow.Web.Logging.Controllers
{
    [Authorize(Roles = RoleConstants.Api)]
    public class LoggingController : Controller
    {
        // GET: LoggingView
        public ActionResult Index()
        {
            return View();
        }
    }
}