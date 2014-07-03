using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Eve.Database;
using Eve.Domain;

namespace EProfit.Controllers
{
    public class IgnoreController : Controller
    {
        public ActionResult Add(long orderId)
        {
            using (var context = new EveHistoryContext())
            {
                context.OrdersToIgnore.Add(new OrderToIgnore{IgnoreDate = DateTime.Now, OrderID = orderId});
                context.SaveChanges();
            }
            return new EmptyResult();
        }
    }
}
