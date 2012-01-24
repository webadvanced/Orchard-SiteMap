using System.Web.Mvc;
using Orchard.Themes;

namespace Orchard.Tokens.Controllers {
    public class AdminController : Controller {

        [Themed(false)]
        public ActionResult List() {
            return View();
        }
    }
}