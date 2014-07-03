<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<EProfit.Controllers.BrowseIndexModel>" %>

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
    
<table>
<tbody>
<% foreach (var item in Model.Items)
   { %>
<tr>
<td> <%= Html.ActionLink(item.Name, "OrdersList", new {typeId = item.TypeId}) %> </td>
</tr>
   <% } %>
    </tbody>
    </table>
</asp:Content>
