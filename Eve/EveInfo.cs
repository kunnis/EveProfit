using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eve
{
    public class EveInfo
    {
        public int[] GetListOfTypesToRefresh(string solarSystemName)
        {
            List<int> result = new List<int>();
            using (var connection = new SqlConnection("Data Source=.\\sqlexpress;Initial Catalog=eve;Integrated Security=SSPI;"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.Parameters.AddWithValue("solarSystemName", solarSystemName);
                    command.CommandText =
                        @"

;with excludes as (
select typeID from EveToolkit..invTypes where marketGroupID in
(
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 2
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 2)
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 2))
UNION
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 350001
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 350001)
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 350001))
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 350001)))
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 350001))))
)
)

select itemid from
(

--Find 50 old items
select itemid from
(
select top 50 itemid from
(
select itemid, timestamp from
(
select itemid, timestamp from sellorders
where stationid in ( select staStations.stationID from EveToolkit..staStations where regionID in ( select regionID from EveToolkit..mapSolarSystems where mapSolarSystems.solarSystemName = @solarSystemName ) )
and itemid not in (select typeID from excludes)
) a
UNION
select itemid, timestamp from
(
select itemid, timestamp from buyorders
where stationid in ( select staStations.stationID from EveToolkit..staStations where regionID in ( select regionID from EveToolkit..mapSolarSystems where mapSolarSystems.solarSystemName = @solarSystemName ) )
and itemid not in (select typeID from excludes)
) b
) stuffToUpdate
where 1 = 0
group by itemid
order by MIN(timestamp)
) ordersThatWillBeUpdated

UNION --plus 50 random items
select typeID as id From
(
select top 50 typeID from EveToolkit..invTypes where marketGroupID is not null and typeID not in (select typeID from excludes) order by NEWID()
) c
) unrandomized
order by NEWID()


";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            result.Add((int)reader["itemid"]);
                    }
                }
            }
            return result.ToArray();
        }

        public int[] GetListOfTypesToRefreshAll()
        {
            List<int> result = new List<int>();
            using (var connection = new SqlConnection("Data Source=.\\sqlexpress;Initial Catalog=eve;Integrated Security=SSPI;"))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        @"

;with excludes as (
select typeID from EveToolkit..invTypes where marketGroupID in
(
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 2
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 2)
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 2))
UNION
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 350001
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 350001)
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 350001))
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 350001)))
union
select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID in (select marketGroupID from EveToolkit..invMarketGroups where parentGroupID = 350001))))
)
)

select typeID as itemid from
(
select typeID from EveToolkit..invTypes where marketGroupID is not null and typeID not in (select typeID from excludes) 
) unrandomized
order by NEWID()


";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            result.Add((int)reader["itemid"]);
                    }
                }
            }
            return result.ToArray();
        }
    }
}
