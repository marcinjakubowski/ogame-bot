using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Engine;
using OgameBot.Proxy;
using OgameBot.Objects.Types;

namespace OgameBot.Engine.Injects
{
    public class CommonInject : IInject
    {
        const string ogbcmdFunction = @"
function ogbcmd(cmd) {
    $.get('/ogbcmd/'+cmd, function() { fadeBox('OK'); }).fail(function() { fadeBox('Error occured.', 1); });
}
";
        const string auctioneerStatus = @"
$('#bar li:last').after('<li style=""float:right;width:100px"" id=ogbAuctioneer>&nbsp;</li>');
$.post('/game/index.php?page=traderOverview', {show: 'auctioneer', ajax: 1}, function(data) { 
    var timeleft = $(data).find('p.auction_info span[style]'); 
    if (timeleft[0]) 
        $('#ogbAuctioneer').html('<a href=""/game/index.php?page=traderOverview#animation=false&page=traderAuctioneer"">Auctioneer: '+timeleft[0].outerHTML.replace('approx. ', '')+'</a>');
    }
);
";
        public string Inject(OgamePageInfo info, string body, ResponseContainer response, string host, int port)
        {
            if (info?.Page != null)
            {
                body = body.Replace(@"</body>", $@"<script type=""text/javascript"">
                                                        ogameUrl = 'http://{host}:{port}';
                                                        {ogbcmdFunction};
                                                        {(info.Page != PageType.Merchant ? auctioneerStatus : string.Empty)};
                                                        </script></body>");
            }

            body = body.Replace("no-commander", "");

            return body;
        }
    }
}
