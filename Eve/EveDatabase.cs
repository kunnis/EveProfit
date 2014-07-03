using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using Eve.Domain;

namespace Eve
{
    public static class EveDatabase
    {
        public static void AddOrdersToDatabase(SqlConnection connection, IEnumerable<OrderSet> ordersSetsToAdd, SqlTransaction transaction, byte source)
        {
            DataTable buyDataTable = new DataTable();
            buyDataTable.Columns.Add("itemid", typeof(int));
            buyDataTable.Columns.Add("orderid", typeof(long));
            buyDataTable.Columns.Add("stationid", typeof(int));
            buyDataTable.Columns.Add("price", typeof(decimal));
            buyDataTable.Columns.Add("volume", typeof(int));
            buyDataTable.Columns.Add("minvolume", typeof(int));
            buyDataTable.Columns.Add("range", typeof(int));
            buyDataTable.Columns.Add("timestamp", typeof(DateTime));
            buyDataTable.Columns.Add("source", typeof(byte));

            DataTable sellDataTable = new DataTable();
            sellDataTable.Columns.Add("itemid", typeof(int));
            sellDataTable.Columns.Add("orderid", typeof(long));
            sellDataTable.Columns.Add("stationid", typeof(int));
            sellDataTable.Columns.Add("price", typeof(decimal));
            sellDataTable.Columns.Add("volume", typeof(int));
            sellDataTable.Columns.Add("minvolume", typeof(int));
            sellDataTable.Columns.Add("range", typeof(int));
            sellDataTable.Columns.Add("timestamp", typeof(DateTime));
            sellDataTable.Columns.Add("source", typeof(byte));

            foreach (OrderSet orderSetToAdd in ordersSetsToAdd)
            {
                foreach (Order order in orderSetToAdd.Orders)
                {
                    //Make sure some of the conversions will go through...
// ReSharper disable ObjectCreationAsStatement
                    try
                    {
                        new SqlMoney(order.Price);
                    }
                    catch (ArithmeticException)
                    {
                        Console.WriteLine("Order with SqlMoney failed to parse: " + order.Price.ToString());
                        continue;
                    }
// ReSharper restore ObjectCreationAsStatement
                    if (order.Bid)
                    {
                        if (buyDataTable.Rows.Cast<DataRow>().Any(
                            row => (int) row["itemid"] == orderSetToAdd.TypeId
                                   && (long) row["orderid"] == order.OrderID
                                   && (int) row["stationid"] == order.StationID
                            ))
                        {
                            Console.WriteLine("Duplicate data detected on buy: itemId:{0} orderId:{1} stationId:{2}", orderSetToAdd.TypeId, order.OrderID, order.StationID);
                            continue;
                        }
                        DataRow buyDataRow = buyDataTable.NewRow();
                        buyDataRow["itemid"] = orderSetToAdd.TypeId;
                        buyDataRow["orderid"] = order.OrderID;
                        buyDataRow["stationid"] = order.StationID;
                        buyDataRow["price"] = order.Price;
                        buyDataRow["volume"] = order.VolRemaining;
                        buyDataRow["minvolume"] = order.MinVolume;
                        buyDataRow["range"] = order.Range;
                        buyDataRow["timestamp"] = orderSetToAdd.GeneratedAt;
                        buyDataRow["source"] = source;
                        buyDataTable.Rows.Add(buyDataRow);
                    }
                    else
                    {
                        if (sellDataTable.Rows.Cast<DataRow>().Any(
                            row => (int)row["itemid"] == orderSetToAdd.TypeId
                                   && (long)row["orderid"] == order.OrderID
                                   && (int)row["stationid"] == order.StationID
                            ))
                        {
                            Console.WriteLine("Duplicate data detected on sell: itemId:{0} orderId:{1} stationId:{2}", orderSetToAdd.TypeId, order.OrderID, order.StationID);
                            continue;
                        }
                        DataRow sellDataRow = sellDataTable.NewRow();
                        sellDataRow["itemid"] = orderSetToAdd.TypeId;
                        sellDataRow["orderid"] = order.OrderID;
                        sellDataRow["stationid"] = order.StationID;
                        sellDataRow["price"] = order.Price;
                        sellDataRow["volume"] = order.VolRemaining;
                        sellDataRow["minvolume"] = order.MinVolume;
                        sellDataRow["range"] = order.Range;
                        sellDataRow["timestamp"] = orderSetToAdd.GeneratedAt;
                        sellDataRow["source"] = source;
                        sellDataTable.Rows.Add(sellDataRow);
                    }
                }
            }

            var sellBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);

            sellBulkCopy.ColumnMappings.Add("itemid", "itemid");
            sellBulkCopy.ColumnMappings.Add("orderid", "orderid");
            sellBulkCopy.ColumnMappings.Add("stationid", "stationid");
            sellBulkCopy.ColumnMappings.Add("price", "price");
            sellBulkCopy.ColumnMappings.Add("volume", "volume");
            sellBulkCopy.ColumnMappings.Add("minvolume", "minvolume");
            sellBulkCopy.ColumnMappings.Add("range", "range");
            sellBulkCopy.ColumnMappings.Add("timestamp", "timestamp");
            sellBulkCopy.ColumnMappings.Add("source", "source");

            sellBulkCopy.BatchSize = 10000;
            //bulkCopy.BulkCopyTimeout = int.Parse(ConfigurationSettings.AppSettings["timeout"]);
            sellBulkCopy.DestinationTableName = "sellorders";
            sellBulkCopy.WriteToServer(sellDataTable.CreateDataReader());

            var buyBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction);

            buyBulkCopy.ColumnMappings.Add("itemid", "itemid");
            buyBulkCopy.ColumnMappings.Add("orderid", "orderid");
            buyBulkCopy.ColumnMappings.Add("stationid", "stationid");
            buyBulkCopy.ColumnMappings.Add("price", "price");
            buyBulkCopy.ColumnMappings.Add("volume", "volume");
            buyBulkCopy.ColumnMappings.Add("minvolume", "minvolume");
            buyBulkCopy.ColumnMappings.Add("range", "range");
            buyBulkCopy.ColumnMappings.Add("timestamp", "timestamp");
            buyBulkCopy.ColumnMappings.Add("source", "source");

            buyBulkCopy.BatchSize = 10000;
            //bulkCopy.BulkCopyTimeout = int.Parse(ConfigurationSettings.AppSettings["timeout"]);
            buyBulkCopy.DestinationTableName = "buyorders";
            buyBulkCopy.WriteToServer(buyDataTable.CreateDataReader());
        }

        public static void WriteOrdersUpdateToDatabase(OrdersUpdate marketUpdate, byte source)
        {
            using (var connection =
                new SqlConnection("Data Source=.\\sqlexpress;Initial Catalog=eve;Integrated Security=SSPI;"))
            {
                connection.Open();


                /*
                string sql =
                    string.Format(
                        @"
    delete from sellorders from sellorders
    inner join staStations on sellorders.stationid = staStations.stationID
    where staStations.regionID in ( select regionID from
    staStations where stationID in ({0})
    )
 
    delete from buyorders from buyorders
    inner join staStations on buyorders.stationid = staStations.stationID
    where staStations.regionID in ( select regionID from
    staStations where stationID in ({0})
    )", string.Join(",", ordersToAdd.Select(o => o.StationID.ToString()).Distinct()));
                 */

                var orderSetsToApply =
                    marketUpdate.OrderSets.Where(
                        orderSet => orderSet.GeneratedAt >= DateTime.Now.AddHours(-24))
                        .ToList();

                //If it's too old, just skip this record.
                if (!orderSetsToApply.Any())
                {
                    Console.WriteLine("Skipping really old record from {0}",
                                      marketUpdate.OrderSets.Max(os => os.GeneratedAt));
                    return;
                }

                orderSetsToApply =
                    marketUpdate.OrderSets.Where(
                        orderSet => orderSet.GeneratedAt >= DateTime.Now.AddMinutes(-5))
                        .ToList();

                //If it's too old, just skip this record.
                if (!orderSetsToApply.Any())
                {
                    Console.WriteLine("Skipping old record from {0}",
                                      marketUpdate.OrderSets.Max(os => os.GeneratedAt));
                    return;
                }

                if (marketUpdate.OrderSets.Count != orderSetsToApply.Count)
                    Console.WriteLine("Mixed data!");

                Console.WriteLine("Loading data from {0}", orderSetsToApply.Min(os => os.GeneratedAt));

                var regionIds = orderSetsToApply.Where(os => os.RegionId.HasValue)
                    .Select(os => new { os.TypeId, RegionId = os.RegionId.Value })
                    .Distinct()
                    .Select(d => new RegionAndTypeId(d.RegionId, d.TypeId))
                    .ToList();

                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    foreach (RegionAndTypeId regionAndTypeId in regionIds)
                    {
                        const string sql = @"
delete from sellorders from sellorders
inner join EveToolkit..staStations on sellorders.stationid = staStations.stationID
where staStations.regionID = @regionId and sellorders.itemid = @typeID
 
delete from buyorders from buyorders
inner join EveToolkit..staStations on buyorders.stationid = staStations.stationID
where staStations.regionID = @regionId and buyorders.itemid = @typeId
";

                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = sql;
                            command.Parameters.AddWithValue("typeid", regionAndTypeId.TypeId);
                            command.Parameters.AddWithValue("regionId", regionAndTypeId.RegionId);
                            command.ExecuteNonQuery();
                        }
                    }

                    foreach (var typeId in regionIds.Select(reg => reg.TypeId).Distinct())
                    {
                        RunCleanupForType(connection, transaction, typeId);
                    }

                    AddOrdersToDatabase(connection, orderSetsToApply, transaction, source);

                    transaction.Commit();
                }
            }
        }

        public static void RunCleanupForType(SqlConnection connection, SqlTransaction transaction, int typeId)
        {
            const string cleanup = @"
delete from sellorders from sellorders
where sellorders.itemid = @typeID and sellorders.stationid not in ( select staStations.stationID from EveToolkit..staStations )
 
delete from buyorders from buyorders
where buyorders.itemid = @typeID and buyorders.stationid not in ( select staStations.stationID from EveToolkit..staStations )";

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = cleanup;
                command.Parameters.AddWithValue("typeid", typeId);
                command.ExecuteNonQuery();
            }
        }
    }
}