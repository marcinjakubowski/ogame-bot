using OgameBot.Engine.Parsing.Objects;
using OgameBot.Engine.Tasks;
using ScraperClientLib.Engine;
using System.Text.RegularExpressions;

namespace OgameBot.Engine.Injects
{
    public class AuctionStatusInject : IInject
    {
        private static Regex clockRegex = new Regex(@"(<li class=""OGameClock.*</li>)", RegexOptions.Compiled);

        private AuctionMonitor _monitor;
        private AuctionStatus Auction => _monitor.Auction;

        public AuctionStatusInject(AuctionMonitor monitor)
        {
            _monitor = monitor;
        }

        public string Inject(OgamePageInfo info, string body, ResponseContainer response, string host, int port)
        {
            if (info?.Page == null) return body;


            string color;

            if ((Auction?.MinutesRemaining ?? 0) == 0) return body;
            else if (Auction.MinutesRemaining >= 20) color = "color:#99CC00;";
            else if (Auction.MinutesRemaining >= 10) color = "color:#ffa500";
            else if (Auction.MinutesRemaining == 5) color = "color:#ff0000";
            else color = string.Empty;

            body = clockRegex.Replace(body, $"$1<li style='float:right'><a href='/game/index.php?page=traderOverview#animation=false&page=traderAuctioneer' class='tooltip' title='{Auction.Item} @ {Auction.CurrentBid}'>Auctioneer: <span style='{color};font-weight:bold;'>{Auction.MinutesRemaining}m</span></a></li>");
            return body;
        }
    }
}
