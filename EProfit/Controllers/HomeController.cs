using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Eve;

namespace EProfit.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //foreach (var key in Request.Headers.AllKeys)
            //{
            //    ViewBag.Message += key + ":" + Request.Headers[key]+"    ";
            //}
            
            return View();
        }

        public ActionResult ProfitableTrades(decimal space, decimal capital, decimal tax, string filter)
        {
            var location = Request.Headers["EVE_SOLARSYSTEMNAME"];

            if (location == null)
            {
                location = "Jita";
            }

            ProfitLocator locator = new ProfitLocator();
            ViewBag.ProfitableTrades = locator.GetProfitableTrades(location, space, capital, tax, (FilterOptions)Enum.Parse(typeof(FilterOptions), filter));
            
            
            var name = Request.Headers["EVE_CHARNAME"];
            if (name == null)
                name = "Unknown";

            ViewBag.Name = name;

            return View();
        }
    }
}
