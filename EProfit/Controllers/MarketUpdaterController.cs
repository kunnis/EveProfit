using System.Web.Mvc;
using Eve;

namespace EProfit.Controllers
{
    public class MarketUpdaterController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MarketRefresh()
        {
            var location = Request.Headers["EVE_SOLARSYSTEMNAME"];

            if (location == null)
            {
                location = "Jita";
            }

            var model = new EveInfo().GetListOfTypesToRefresh(location);
            return Json(model);
        }
    }
}