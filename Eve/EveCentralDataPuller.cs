using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Xml;
using Eve.Domain;
using System.Linq;

namespace Eve
{
    class EveCentralDataPuller
    {
        private static void Main(string[] args)
        {
            /*
<evec_api method="quicklook" version="2.0">
  <quicklook>
    <item>34</item>
    <itemname>Tritanium</itemname>
    <regions/>
    <hours>360</hours>
    <minqty>10001</minqty>
    <sell_orders>
      <order id="2634176553">
        <region>10000038</region>
        <station>60006829</station>
        <station_name>Kuomi VI - Moon 2 - Ducia Foundry Refinery</station_name>
        <security>0.567863363808865</security>
        <range>32767</range>
        <price>6.00</price>
        <vol_remain>4139299</vol_remain>
        <min_volume>1</min_volume>
        <expires>2012-08-25</expires>
        <reported_time>08-24 18:45:13</reported_time>
      </order>
    </sell_orders>
    <buy_orders>
      <order id="2406576519">
        <region>10000038</region>
        <station>60002197</station>
        <station_name>Kuomi VIII - Moon 22 - Ishukone Corporation Factory</station_name>
        <security>0.567863363808865</security>
        <range>1</range>
        <price>4.75</price>
        <vol_remain>1745863</vol_remain>
        <min_volume>1</min_volume>
        <expires>2012-08-25</expires>
        <reported_time>08-24 18:45:13</reported_time>
      </order>
    </buy_orders>
  </quicklook>
</evec_api>
             */

            var connection = new SqlConnection("Data Source=.\\sqlexpress;Initial Catalog=eve;Integrated Security=SSPI;");
            connection.Open();

            int[] typeIds = new EveInfo().GetListOfTypesToRefreshAll();
            //int[] typeIds = { 34 };

            int index = 0;
            foreach (var typeId in typeIds)
            {
                index++;
                UpdateInfoForType(connection, typeId);
                Console.WriteLine(" {0} of {1}", index, typeIds.Length);
            }

            Console.WriteLine("Done.");
            Console.ReadKey();
        }

        private static void UpdateInfoForType(SqlConnection connection, int typeId)
        {
            WebClient client = new WebClient();
            client.Headers["User-Agent"] = "";
            string xml;
            try
            {
                xml = client.DownloadString("http://api.eve-central.com/api/quicklook?sethours=24&typeid=" + typeId);
            }
            catch (WebException ex)
            {
                Console.WriteLine("Got an error trying to download data: " + ex.Message);
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            if (doc.SelectSingleNode("/evec_api/quicklook/item") == null)
            {
                Console.WriteLine("UpdateInfoForType got back a response that doesn't have an item for typeId: {0}", typeId);
                Console.WriteLine(xml);
                return;
            }

            string itemid = doc.SelectSingleNode("/evec_api/quicklook/item").InnerText.Trim();
            string itemname = doc.SelectSingleNode("/evec_api/quicklook/itemname").InnerText.Trim();
            Console.Write("Loading {0}.", itemname);

            var ordersToAdd = new List<OrderSet>();

            foreach (XmlElement orderNode in doc.SelectNodes("/evec_api/quicklook/sell_orders/order"))
            {
                int regionId = Int32.Parse(orderNode.SelectSingleNode("region").InnerText.Trim());
                DateTime reported = DateTime.ParseExact(orderNode.SelectSingleNode("reported_time").InnerText.Trim(), "MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                reported = new DateTime(2013, reported.Month, reported.Day, reported.Hour, reported.Minute, reported.Second);
                OrderSet orderSet = ordersToAdd.SingleOrDefault(os => os.RegionId == regionId && os.TypeId == typeId);
                if (orderSet == null)
                {
                    orderSet = new OrderSet();
                    orderSet.GeneratedAt = reported;
                    orderSet.TypeId = typeId;
                    orderSet.RegionId = regionId;
                    orderSet.Orders = new List<Order>();
                    ordersToAdd.Add(orderSet);
                }
                if (reported < orderSet.GeneratedAt)
                    orderSet.GeneratedAt = reported;
                var o = ParseOrderNode(typeId, orderNode, false);
                if(o != null)
                    orderSet.Orders.Add(o);
            }

            foreach (XmlElement orderNode in doc.SelectNodes("/evec_api/quicklook/buy_orders/order"))
            {
                int regionId = Int32.Parse(orderNode.SelectSingleNode("region").InnerText.Trim());
                DateTime reported = DateTime.ParseExact(orderNode.SelectSingleNode("reported_time").InnerText.Trim(), "MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                
                reported = new DateTime(2013, reported.Month, reported.Day, reported.Hour, reported.Minute, reported.Second);
                OrderSet orderSet = ordersToAdd.SingleOrDefault(os => os.RegionId == regionId && os.TypeId == typeId);
                if (orderSet == null)
                {
                    orderSet = new OrderSet();
                    orderSet.GeneratedAt = reported;
                    orderSet.TypeId = typeId;
                    orderSet.RegionId = regionId;
                    orderSet.Orders = new List<Order>();
                    ordersToAdd.Add(orderSet);
                }
                if (reported < orderSet.GeneratedAt)
                    orderSet.GeneratedAt = reported;
                var o = ParseOrderNode(typeId, orderNode, true);
                if(o != null)
                    orderSet.Orders.Add(o);
            }

            using (var transaction = connection.BeginTransaction())
            {
                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "delete from sellorders where itemid = @itemid";
                    command.Parameters.AddWithValue("itemid", itemid);
                    command.ExecuteNonQuery();
                }

                using (var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = "delete from buyorders where itemid = @itemid";
                    command.Parameters.AddWithValue("itemid", itemid);
                    command.ExecuteNonQuery();
                }

                EveDatabase.AddOrdersToDatabase(connection, ordersToAdd, transaction, 1);
                EveDatabase.RunCleanupForType(connection, transaction, typeId);
                transaction.Commit();
            }
        }

        private static Order ParseOrderNode(int typeId, XmlElement orderNode, bool isBidOrder)
        {
            /*
                 <region>10000038</region>
                    <station>60006829</station>
                    <station_name>Kuomi VI - Moon 2 - Ducia Foundry Refinery</station_name>
                    <security>0.567863363808865</security>
                    <range>32767</range>
                    <price>6.00</price>
                    <vol_remain>4139299</vol_remain>
                    <min_volume>1</min_volume>
                    <expires>2012-08-25</expires>
                    <reported_time>08-24 18:45:13</reported_time>
            */

            long orderId = Int64.Parse(orderNode.Attributes["id"].Value);
            int region = Int32.Parse(orderNode.SelectSingleNode("region").InnerText.Trim());
            int station = Int32.Parse(orderNode.SelectSingleNode("station").InnerText.Trim());
            
            //For some reason I was getting orders with a GIANT price back from the API.  I'm skipping these.  1/18/2013
            decimal price;
            if (!Decimal.TryParse(orderNode.SelectSingleNode("price").InnerText.Trim(), out price))
            {
                Console.WriteLine("Found a bad order.  Price was: {0}  Skipping.", orderNode.SelectSingleNode("price").InnerText.Trim());
                return null;
            }
            int volRemain = Int32.Parse(orderNode.SelectSingleNode("vol_remain").InnerText.Trim());
            int minVolume = Int32.Parse(orderNode.SelectSingleNode("min_volume").InnerText.Trim());
            int range = Int32.Parse(orderNode.SelectSingleNode("range").InnerText.Trim());
            //DateTime expires = DateTime.Parse(orderNode.SelectSingleNode("expires").InnerText.Trim());
            //DateTime reported = DateTime.Parse(orderNode.SelectSingleNode("reported_time").InnerText.Trim());

            //Console.WriteLine("id: {0}  price: {1}  remaining: {2}", orderId, price, volRemain);
            Order o = new Order
                          {
                              MinVolume = minVolume,
                              OrderID = orderId,
                              Price = price,
                              StationID = station,
                              VolRemaining = volRemain,
                              Bid = isBidOrder,
                              //TypeId = typeId,
                              Range = range,
                          };
            return o;
        }
    }
}