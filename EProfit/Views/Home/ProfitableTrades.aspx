<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %><%
  var name = (string)ViewBag.Name;
var trades = (List<Eve.ProfitLocator.TradeInfo>)ViewBag.ProfitableTrades;
  if (trades != null)
  { %>
<script type="text/javascript">
    _gaq.push(['_trackEvent', 'ProfitableTrades', 'Loaded', '<%=name%>']);
</script>
<table>
<tbody>
<% foreach (var tradeInfo in trades)
   { %>
<tr>
<td><a href="#"><span class="clickToMarketDetails" data-typeid="<%= tradeInfo.TypeId %>"><%= tradeInfo.TypeName %></span></a></td>
<td><%= tradeInfo.DistanceToStart %></td>
<td><%= tradeInfo.DistBetweenBuyAndSell %></td>
<td style="border-left-width: 2px; border-left-color: black; "><%= tradeInfo.SellPrice.ToString("C") %></td>
<td>
<%= tradeInfo.SellVolume %>
<% if (tradeInfo.SellMinVolume > 1)
   { %>
   <span style="color:red; font-weight: bold">
   Min: <%= tradeInfo.SellMinVolume %>
   </span>
   <% } %>
</td>
<td><a href="#"><span class="clickToSetDestination" data-stationId="<%= tradeInfo.SellStationId %>"><%= tradeInfo.SellStationName %></span></a></td>
<td style="border-left-width: 2px; border-left-color: black; "><%= tradeInfo.BuyPrice.ToString("C") %></td>
<td>
<%= tradeInfo.BuyVolume %>
<% if (tradeInfo.BuyMinVolume > 1)
   { %>
   <span style="color:red; font-weight: bold">
   Min: <%= tradeInfo.BuyMinVolume %>
   </span>
<% } %>
</td>
<td><a href="#"><span class="clickToSetDestination" data-stationId="<%= tradeInfo.BuyStationId %>"><%= tradeInfo.BuyStationName %></span></a></td>
<td style="border-left-width: 2px; border-left-color: black; "><%= tradeInfo.HaulQuantity %></td>
<td><%= tradeInfo.HaulVolume %> m3</td>
<td><%= tradeInfo.Profitperjump.ToString("C") %></td>
<td><%= tradeInfo.EstProfit.ToString("C") %></td>
</tr>
<% } %>
</tbody>
</table>
<% } %>