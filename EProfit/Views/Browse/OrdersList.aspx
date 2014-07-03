<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<EProfit.Controllers.BrowseOrdersListModel>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript">


        var _gaq = _gaq || [];
        _gaq.push(['_setAccount', 'UA-36982455-1']);
        _gaq.push(['_trackPageview']);

        (function () {
            var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
            ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
            var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
        })();
    </script>
    Orders seen in the past 24 hours...
    <h1>Sell Orders</h1>
    <table>
        <thead>
            <tr>
                <th>Station Name</th>
                <th>Price</th>
                <th>Quantity</th>
                <th>Reported Time UTC</th>
            </tr>
        </thead>
        <tbody>
            <% foreach (var order in Model.Sells)
               { %>
            <tr>
                <td><%= order.StationName %></td>
                <td><%= order.Price %></td>
                <td><%= order.Volume %></td>
                <td><%= order.TimeStamp %></td>
            </tr>
                <% } %>
        </tbody>
    </table>
    
    <h1>Buy Orders</h1>
    <table>
        <thead>
            <tr>
                <th>Station Name</th>
                <th>Price</th>
                <th>Quantity</th>
                <th>Reported Time</th>
            </tr>
        </thead>
        <tbody>
            <% foreach (var order in Model.Buys)
               { %>
            <tr>
                <td><%= order.StationName %></td>
                <td><%= order.Price %></td>
                <td><%= order.Volume %></td>
                <td><%= order.TimeStamp %></td>
            </tr>
                <% } %>
        </tbody>
    </table>
</asp:Content>
