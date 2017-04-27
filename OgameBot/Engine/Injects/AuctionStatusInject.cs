using OgameBot.Engine.Parsing.Objects;
using OgameBot.Engine.Tasks;
using ScraperClientLib.Engine;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

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
            if (info?.Page == null || (Auction?.MinutesRemaining ?? 0) == 0) return body;


            string color;

            if (Auction.MinutesRemaining >= 20) color = "color:#99CC00;";
            else if (Auction.MinutesRemaining >= 10) color = "color:#ffa500";
            else if (Auction.MinutesRemaining == 5) color = "color:#ff0000";
            else color = string.Empty;

            StringBuilder tooltipBuilder = new StringBuilder($"<b>{Auction.Item}</b><br/><b>Bid:</b> {Auction.CurrentBid}<br/><b>Bidder:</b> {Auction.HighestBidderName}<hr/><b>Bid using: </b>");
            tooltipBuilder.Append(InjectHelper.GenerateCommandLink($"bid?cp={info.PlanetId}&resource=m", "Metal")).Append(' ')
                   .Append(InjectHelper.GenerateCommandLink($"bid?cp={info.PlanetId}&resource=c", "Crystal")).Append(' ')
                   .Append(InjectHelper.GenerateCommandLink($"bid?cp={info.PlanetId}&resource=d", "Deuterium"));

            string tooltip = InjectHelper.EncodeTooltip(tooltipBuilder.ToString());

            body = clockRegex.Replace(body, $"$1<li style='float:right'><a href='/game/index.php?page=traderOverview#animation=false&page=traderAuctioneer' class='tooltipClose' title='{tooltip}'>Auctioneer: <span style='{color};font-weight:bold;'>{Auction.MinutesRemaining}m</span></a></li>");
            return body;
        }
    }
}
