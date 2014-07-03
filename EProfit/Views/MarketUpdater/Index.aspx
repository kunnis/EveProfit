<%@ Page Title="Title" Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" MasterPageFile="~/Views/Shared/Site.Master" %>



<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Market Updater
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
        <script language="javascript">
            var typeIds = null;

            function loadNextData() {
                $.ajax({
                    url: '<%= Url.Action("MarketRefresh") %>',
                    type: "POST",
                    success: function(data) {
                        typeIds = data;
                    },
                    error: function() {
                        alert("fail :-(  The MarketRefresh action returned an error.");
                    }
                });
            }

            loadNextData();

            window.setInterval(function(){
                if (typeIds == null || typeIds.length == 0) {
                    $('#data').text('Loading... ');
                    return;
                }

                var nextToShow = typeIds.pop();
                if (typeof CCPEVE !== 'undefined')
                    CCPEVE.showMarketDetails(nextToShow);
                $('#data').text('Loading item ' + nextToShow);
                
                if (typeIds.length == 0) {
                    loadNextData();
                }
            }, 3200);
            
        </script>
        <div id="data"></div>
</asp:Content>