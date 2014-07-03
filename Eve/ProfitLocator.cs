using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Eve
{
    public class ProfitLocator
    {
        private static string commandText = @"


; with selldata as
(
select sellorders.itemid, sellorders.minvolume, sellorders.price, sellorders.stationid, sellorders.volume, sellorders.orderid from sellorders
where
sellorders.timestamp > DATEADD(HH, -24, GETUTCDATE())
),
buydata as
(
select buyorders.itemid, buyorders.minvolume, buyorders.price, buyorders.price * (1-@tax) as priceMinusTax, buyorders.stationid, buyorders.volume, buyorders.orderid from buyorders
where (buyorders.minvolume < buyorders.volume *.50 OR buyorders.minvolume = 1 OR (buyorders.minvolume * buyorders.price) < 1000000) --Don't pick up the buy/sale scams in jita
and buyorders.timestamp > DATEADD(HH, -24, GETUTCDATE())
),
itemdata as
(
select invTypes.typeID, invTypes.typeName, case when invTypes.volume = 0 then 0.00000001 else CASE groupID
 WHEN 324 THEN 2500 -- AssaultShip
 WHEN 448 THEN 1000 -- AuditLogSecureContainer
 WHEN 419 THEN 15000 -- Battlecruiser
 WHEN 27 THEN 50000 -- Battleship
 WHEN 898 THEN 50000 -- BlackOps
 WHEN 883 THEN 1000000 -- CapitalIndustrialShip
 WHEN 29 THEN 500 -- Capsule
 WHEN 12 THEN 1000 -- CargoContainer
 WHEN 547 THEN 1000000 -- Carrier
 WHEN 906 THEN 10000 -- CombatReconShip
 WHEN 540 THEN 15000 -- CommandShip
 WHEN 830 THEN 2500 -- CovertOps
 WHEN 26 THEN 10000 -- Cruiser
 WHEN 420 THEN 5000 -- Destroyer
 WHEN 485 THEN 1000000 -- Dreadnought
 WHEN 893 THEN 2500 -- ElectronicAttackShips
 WHEN 381 THEN 50000 -- EliteBattleship
 WHEN 543 THEN 3750 -- Exhumer
 WHEN 833 THEN 10000 -- ForceReconShip
 WHEN 649 THEN 1000 -- FreightContainer
 WHEN 513 THEN 1000000 -- Freighter
 WHEN 25 THEN 2500 -- Frigate
 WHEN 358 THEN 10000 -- HeavyAssaultShip
 WHEN 894 THEN 10000 -- HeavyInterdictors
 WHEN 28 THEN 20000 -- Industrial
 WHEN 941 THEN 500000 -- IndustrialCommandShip
 WHEN 831 THEN 2500 -- Interceptor
 WHEN 541 THEN 5000 -- Interdictor
 WHEN 902 THEN 1000000 -- JumpFreighter
 WHEN 832 THEN 10000 -- Logistics
 WHEN 900 THEN 50000 -- Marauders
 WHEN 463 THEN 3750 -- MiningBarge
 WHEN 952 THEN 1000 -- MissionContainer
 WHEN 659 THEN 1000000 -- Mothership - Supercarrier
 WHEN 237 THEN 2500 -- Rookieship
 WHEN 340 THEN 1000 -- SecureCargoContainer
 WHEN 31 THEN 500 -- Shuttle
 WHEN 834 THEN 2500 -- StealthBomber
 WHEN 963 THEN 5000 -- StrategicCruiser
 WHEN 30 THEN 10000000 -- Titan
 WHEN 380 THEN 20000 -- TransportShip 
WHEN 1022 THEN 500 -- Prototype Exploration Ship
 ELSE volume
 END end as volume from EveToolkit..invTypes
)
select top 1000 itemdata.typeID, itemdata.typeName, ssjs.distance as distanceToStart,
ssj.distance as distBetweenBuyAndSell,
selldata.price as sellPrice, selldata.volume as sellVolume, selldata.minvolume as sellMinVolume, sellStation.stationName as sellStationName, sellStation.stationID as sellStationId, selldata.orderid as sellOrderId,
buydata.price as buyPrice, buydata.volume as buyVolume, buydata.minvolume as buyMinVolume, buyStation.stationName as buyStationName, buyStation.stationID as buyStationId, buydata.orderid as buyOrderId,
 (select top 1 mapSolarSystems.solarSystemID from
  EveToolkit..mapSolarSystems
  left join ssj leg1 on leg1.[to] = mapSolarSystems.solarSystemID and leg1.[from] = (select solarSystemID from EveToolkit..mapSolarSystems where solarSystemName = @location)
  left join ssj leg2 on leg2.[from] = mapSolarSystems.solarSystemID and leg2.[to] = buyStation.stationID
  where mapSolarSystems.regionID = buyStation.regionID
  order by leg1.distance + leg2.distance, leg1.distance
  ) as nearestPointToSeeBuyStation,

 dbo.minof2(Floor(@volume / itemdata.volume), dbo.minof2(buydata.volume,dbo.minof2(selldata.volume, floor(@funds/selldata.price)))) as haulQty,
 dbo.minof2(Floor(@volume / itemdata.volume), dbo.minof2(buydata.volume,dbo.minof2(selldata.volume, floor(@funds/selldata.price)))) * itemdata.volume as haulVolume,
   ( ( buydata.priceMinusTax - selldata.price ) / (ssj.distance + ssjs.distance + 2 + @jumppenlty) ) *
 dbo.minof2(Floor(@volume / itemdata.volume), dbo.minof2(buydata.volume,dbo.minof2(selldata.volume, floor(@funds/selldata.price)))) as profitperjump,
   ( ( buydata.priceMinusTax - selldata.price ) ) *
 dbo.minof2(Floor(@volume / itemdata.volume), dbo.minof2(buydata.volume,dbo.minof2(selldata.volume, floor(@funds/selldata.price)))) as estProfit
from buydata
inner join itemdata on itemdata.typeID = buydata.itemid
inner join EveToolkit..staStations buyStation on buydata.stationid = buyStation.stationID
inner join ssj ssj on buyStation.solarSystemID = ssj.[from]
inner join EveToolkit..staStations sellStation on ssj.[to] = sellStation.solarSystemID
--compute distance from our start point here....
inner join ssj ssjs on ssjs.[to] = sellStation.solarSystemID and (ssjs.[from] in (select solarSystemID from EveToolkit..mapSolarSystems where mapSolarSystems.solarSystemName = @location ))
inner join selldata on sellStation.stationID = selldata.stationid and selldata.itemid = buydata.itemid
where ( ( buydata.priceMinusTax - selldata.price ) ) > 0  --and buyStation.stationName like 'Jita%'
AND ( ( buydata.priceMinusTax - selldata.price )  ) *
 dbo.minof2(Floor(@volume / itemdata.volume), dbo.minof2(buydata.volume,dbo.minof2(selldata.volume, floor(@funds/selldata.price)))) > 1000000
 and dbo.minof2(Floor(@volume / itemdata.volume), dbo.minof2(buydata.volume,dbo.minof2(selldata.volume, floor(@funds/selldata.price)))) >= buydata.minvolume
 and dbo.minof2(Floor(@volume / itemdata.volume), dbo.minof2(buydata.volume,dbo.minof2(selldata.volume, floor(@funds/selldata.price)))) >= selldata.minvolume

{0}
order by ( ( buydata.priceMinusTax - selldata.price )  ) *
 dbo.minof2(Floor(@volume / itemdata.volume), dbo.minof2(buydata.volume,dbo.minof2(selldata.volume, floor(@funds/selldata.price)))) --/ (ssj.distance + ssjs.distance + 2 + @jumppenlty)
 desc




";
        public class TradeInfo
        {
            public string TypeName { get; set; }
            public int DistanceToStart { get; set; }
            public int DistBetweenBuyAndSell { get; set; }
            public decimal SellPrice { get; set; }
            public int SellVolume { get; set; }
            public int SellMinVolume { get; set; }
            public string SellStationName { get; set; }
            public int SellStationId { get; set; }
            public decimal BuyPrice { get; set; }
            public int BuyVolume { get; set; }
            public int BuyMinVolume { get; set; }
            public string BuyStationName { get; set; }
            public int BuyStationId { get; set; }
            public int HaulQuantity { get; set; }
            public decimal HaulVolume { get; set; }
            public decimal Profitperjump { get; set; }
            public decimal EstProfit { get; set; }
            public decimal TypeId { get; set; }
        }

        public List<TradeInfo> GetProfitableTrades(string location, decimal volume, decimal funds, decimal taxRate, FilterOptions filterOption)
        {
            List<TradeInfo> result = new List<TradeInfo>();
            using(var connection = new SqlConnection("Data Source=.\\sqlexpress;Initial Catalog=eve;Integrated Security=SSPI;"))
            {
                string whereFilter;
                switch (filterOption)
                {
                    case FilterOptions.CurrentRegion:
                        whereFilter = "and sellStation.regionID in (select regionID from EveToolkit..mapSolarSystems where solarSystemName = @location)";
                        break;
                    default:
                        whereFilter = string.Empty;
                        break;
                }

                connection.Open();
                using(var command = connection.CreateCommand())
                {
                    command.CommandText = string.Format(commandText, whereFilter);
                    command.CommandTimeout = 300;
                    command.Parameters.AddWithValue("volume", volume);
                    command.Parameters.AddWithValue("funds", funds);
                    command.Parameters.AddWithValue("jumppenlty", 2);
                    command.Parameters.AddWithValue("location", location);
                    command.Parameters.AddWithValue("tax", taxRate);
                    
                    using(var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TradeInfo tradeInfo = new TradeInfo();
                            tradeInfo.TypeName = (string) reader["typeName"];
                            tradeInfo.DistanceToStart = (int) reader["distanceToStart"];
                            tradeInfo.DistBetweenBuyAndSell = (int) reader["distBetweenBuyAndSell"];
                            tradeInfo.SellPrice = (decimal) reader["sellPrice"];
                            tradeInfo.SellVolume = (int) reader["sellVolume"];
                            tradeInfo.SellMinVolume = (int) reader["sellMinVolume"];
                            tradeInfo.SellStationName = (string) reader["sellStationName"];
                            tradeInfo.SellStationId = (int)reader["sellStationId"];
                            tradeInfo.BuyPrice = (decimal) reader["buyPrice"];
                            tradeInfo.BuyVolume = (int) reader["buyVolume"];
                            tradeInfo.BuyMinVolume = (int) reader["buyMinVolume"];
                            tradeInfo.BuyStationName = (string) reader["buyStationName"];
                            tradeInfo.BuyStationId = (int)reader["buyStationId"];
                            tradeInfo.HaulQuantity = (int)(decimal)reader["haulQty"];
                            tradeInfo.HaulVolume = (decimal)(double) reader["haulVolume"];
                            tradeInfo.Profitperjump = (decimal) reader["profitperjump"];
                            tradeInfo.EstProfit = (decimal) reader["estProfit"];
                            tradeInfo.TypeId = (int)reader["typeID"];
                            result.Add(tradeInfo);
                        }
                    }
                }
                return result;
            }
        }
    }

    public enum FilterOptions
    {
        All,
        CurrentRegion
    }
}
