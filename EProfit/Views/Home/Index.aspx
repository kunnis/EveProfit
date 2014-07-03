<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript">
        

  var _gaq = _gaq || [];
  _gaq.push(['_setAccount', 'UA-36982455-1']);
  _gaq.push(['_trackPageview']);

  (function() {
    var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
    ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
    var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
  })();

        
  function loadData() {
      
      //Log it in GA
      _gaq.push(['_trackEvent', 'Home', 'LoadedData']);
      
            var space = $('#space').val();
            var capital = $('#capital').val();
            var tax = $('#tax').val();
            var filter = $('#filter option:selected').val();

            var opts = {
                lines: 13, // The number of lines to draw
                length: 7, // The length of each line
                width: 4, // The line thickness
                radius: 10, // The radius of the inner circle
                corners: 1, // Corner roundness (0..1)
                rotate: 0, // The rotation offset
                color: '#000', // #rgb or #rrggbb
                speed: 1, // Rounds per second
                trail: 60, // Afterglow percentage
                shadow: false, // Whether to render a shadow
                hwaccel: false, // Whether to use hardware acceleration
                className: 'spinner', // The CSS class to assign to the spinner
                zIndex: 2e9, // The z-index (defaults to 2000000000)
                top: 'auto', // Top position relative to parent in px
                left: 'auto' // Left position relative to parent in px
            };
            $('#data').html('');
            var target = document.getElementById('data');
            var spinner = new Spinner(opts).spin(target);
            
            $('#data').load('<%= Url.Action("ProfitableTrades") %>' + '?space=' + space + '&capital=' + capital + '&tax=' + tax + '&filter=' + filter, function () {
                $('.clickToMarketDetails').click(function () {
                    if (typeof CCPEVE !== 'undefined')
                        CCPEVE.showMarketDetails($(this).attr('data-typeid'));
                    return false;
                });
                $('.clickToSetDestination').click(function () {
                    if (typeof CCPEVE !== 'undefined')
                        CCPEVE.setDestination($(this).attr('data-stationId'));
                    return false;
                });
            });
        }

        $(function () {
            loadData();

            $('#load').click(function() {
                loadData();
                return false;
            });
        });
    </script>
        <% if (Request.Headers["EVE_TRUSTED"] != null && Request.Headers["EVE_TRUSTED"] == "No")
           { %>
        <script language="javascript" type="text/javascript">
            CCPEVE.requestTrust('<%= Request.Url.ToString().Substring(0, Request.Url.ToString().IndexOf('/', 8)) %>');
        </script>
        <% } %>
    <div>
        Space: <input type="text" id="space" value="73000"/> m3<br/>
        Capital: <input type="text" id="capital" value="700000000"/> ISK<br/>
        Tax: <input type="text" id="tax" value=".0090"/><br/>
        Sell Filter: <select id="filter">
            <option value="All">All</option>
            <option value="CurrentRegion">Current Region</option>
        </select><br/>
        <input type="submit" value="Load" id="load"/><br/>
    </div>
    <div id="data"></div>
</asp:Content>
