using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EProfit.Controllers
{
    public class BrowseController : Controller
    {
        //
        // GET: /Browse/

        public ActionResult Index()
        {
            var model = BrowseInfo.GetListOfTypesInTheMarket();
            return View(model);
        }

        public ActionResult OrdersList(int typeId)
        {
            var model = BrowseInfo.GetListOfOrders(typeId);

            return View(model);
        }
    }

    public class BrowseOrdersListModel
    {
        public BrowseOrdersListModel()
        {
            Buys = new List<BrowseOrdersListModelOrder>();
            Sells = new List<BrowseOrdersListModelOrder>();
        }

        public List<BrowseOrdersListModelOrder> Sells { get; set; }

        public List<BrowseOrdersListModelOrder> Buys { get; set; }

        public class BrowseOrdersListModelOrder
        {
            public string StationName { get; set; }
            public decimal Price { get; set; }
            public int Volume { get; set; }
            public DateTime TimeStamp { get; set; }
        }
    }

    public class BrowseIndexModel
    {
        public BrowseIndexModel()
        {
            Items = new List<BrowseIndexModelItem>();
        }

        public class BrowseIndexModelItem
        {
            public int TypeId { get; set; }
            public string Name { get; set; }
        }

        public List<BrowseIndexModelItem> Items { get; private set; }
    }

    public static class BrowseInfo
    {
        public static BrowseOrdersListModel GetListOfOrders(int typeId)
        {
            BrowseOrdersListModel model = new BrowseOrdersListModel();
            using (
                var connection =
                    new SqlConnection("Data Source=.\\sqlexpress;Initial Catalog=eve;Integrated Security=SSPI;")
                )
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText =
                        "select stationName, price, volume, timestamp from buyorders inner join EveToolkit..staStations on staStations.stationID = buyorders.stationid where itemid = @typeId and timestamp > DATEADD(HH, -24, GETUTCDATE()) order by price desc";
                    command.Parameters.AddWithValue("typeId", typeId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Buys.Add(new BrowseOrdersListModel.BrowseOrdersListModelOrder
                                {
                                    StationName = (string)reader["stationName"],
                                    Price = (decimal)reader["price"],
                                    Volume = (int)reader["volume"],
                                    TimeStamp = (DateTime)reader["timestamp"]
                                });
                        }
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText =
                        "select stationName, price, volume, timestamp from sellorders inner join EveToolkit..staStations on staStations.stationID = sellorders.stationid where itemid = @typeId and timestamp > DATEADD(HH, -24, GETUTCDATE()) order by price asc";
                    command.Parameters.AddWithValue("typeId", typeId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Sells.Add(new BrowseOrdersListModel.BrowseOrdersListModelOrder
                            {
                                StationName = (string)reader["stationName"],
                                Price = (decimal)reader["price"],
                                Volume = (int)reader["volume"],
                                TimeStamp = (DateTime)reader["timestamp"]
                            });
                        }
                    }
                }
            }

            return model;
        }

        public static BrowseIndexModel GetListOfTypesInTheMarket()
        {
            BrowseIndexModel model = new BrowseIndexModel();
            using (
                var connection =
                    new SqlConnection("Data Source=.\\sqlexpress;Initial Catalog=eve;Integrated Security=SSPI;")
                )
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText =
                        "select typeID, typeName from EveToolkit..invTypes where marketGroupID is not null order by invTypes.typeName";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Items.Add(new BrowseIndexModel.BrowseIndexModelItem() { TypeId = (int)reader["typeID"], Name = (string)reader["typeName"] });
                        }
                    }
                }
            }

            return model;
        }
    }
}
