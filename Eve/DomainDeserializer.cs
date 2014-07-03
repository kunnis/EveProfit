using System;
using System.Collections.Generic;
using System.Linq;
using Eve.Domain;
using Newtonsoft.Json.Linq;

namespace Eve
{
    public static class JTokenExtension
    {
        public static T ValueOrDefault<T>(this JToken token)
        {
            if(token == null)
                return default(T);

            return token.Value<T>();
        }
    }

    public static class DomainDeserializer
    {
        public static OrdersUpdate DeserializeIntoMarketUpdate(JObject dictionary)
        {
            var marketUpdate = new OrdersUpdate();
            marketUpdate.Version = dictionary["version"].Value<string>();

            marketUpdate.UploadKeys =
                dictionary["uploadKeys"].Select(
                    jt => new UploadKey { Name = jt["name"].Value<string>(), Key = jt["key"].Value<string>() })
                    .ToList();

            var generator = dictionary["generator"];
            marketUpdate.GeneratorVersion = generator["version"].ValueOrDefault<string>();
            marketUpdate.GeneratorName = generator["name"].ValueOrDefault<string>();
            marketUpdate.MessageTimeStamp = dictionary["currentTime"].Value<DateTime>();

            int i = 0;
            var columnIndex = new Dictionary<string, int>();
            foreach (JToken jToken in dictionary["columns"])
            {
                columnIndex.Add(jToken.Value<string>(), i);
                i++;
            }

            marketUpdate.OrderSets = new List<OrderSet>();

            foreach (JToken rowset in dictionary["rowsets"])
            {
                var orderSet = new OrderSet();
                orderSet.TypeId = rowset["typeID"].Value<int>();
                orderSet.GeneratedAt = rowset["generatedAt"].Value<DateTime>();
                orderSet.RegionId = rowset["regionID"].Value<int?>();
                orderSet.Orders = new List<Order>();

                foreach (JToken jToken in rowset["rows"])
                {
                    var price = jToken[columnIndex["price"]].Value<decimal>();
                    var volRemaining = jToken[columnIndex["volRemaining"]].Value<int>();
                    var range = jToken[columnIndex["range"]].Value<int>();
                    var orderId = jToken[columnIndex["orderID"]].Value<long>();
                    var volEntered = jToken[columnIndex["volEntered"]].Value<int>();
                    var minVolume = jToken[columnIndex["minVolume"]].Value<int>();
                    var isBid = jToken[columnIndex["bid"]].Value<bool>();
                    var issueDate = jToken[columnIndex["issueDate"]].Value<DateTime>();
                    var duration = jToken[columnIndex["duration"]].Value<int>();
                    var stationId = jToken[columnIndex["stationID"]].Value<int>();
                    var solarSystemId = jToken[columnIndex["solarSystemID"]].Value<int?>();
                    //Console.Write(rowset);

                    var o = new Order
                        {
                            StartingVolume = volEntered,
                            MinVolume = minVolume,
                            OrderID = orderId,
                            Price = price,
                            StationID = stationId,
                            VolRemaining = volRemaining,
                            Bid = isBid,
                            Range = range,
                            IssueDate = issueDate,
                            Duration = duration,
                            SolarSystemId = solarSystemId,
                        };

                    orderSet.Orders.Add(o);
                }
                marketUpdate.OrderSets.Add(orderSet);
            }
            return marketUpdate;
        }
    }
}