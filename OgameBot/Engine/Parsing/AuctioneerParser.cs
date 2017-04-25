using ScraperClientLib.Engine.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScraperClientLib.Engine;
using System.Net.Http;
using OgameBot.Engine.Parsing.Objects;
using System.Text.RegularExpressions;
using OgameBot.Objects.Types;
using System.Globalization;

namespace OgameBot.Engine.Parsing
{
    public class AuctioneerParser : BaseParser
    {
        private static Regex timeParser = new Regex(@"(\d+)(m?)", RegexOptions.Compiled);

        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.AbsoluteUri.Contains("page=traderOverview") && container.RequestMessage.Method == HttpMethod.Post;
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            var doc = container.ResponseHtml.Value.DocumentNode;

            

            var content = doc.SelectSingleNode(".//div[@class='left_content']");

            if (content == null) yield break;

            AuctionStatus status = new AuctionStatus();

            var auctionInfo = content.SelectSingleNode("./p[@class='auction_info']");
            var remaining = auctionInfo.SelectSingleNode("./span")?.InnerText ?? string.Empty;

            Match match = timeParser.Match(remaining);
            
            if (match.Success)
            {
                var time = int.Parse(match.Groups[1].Value);
                if (match.Groups[2].Value == "m")
                    status.MinutesRemaining = time;
                else
                    status.NextIn = TimeSpan.FromSeconds(time);
            }

            var bid = content.SelectSingleNode("./div[contains(@class, 'currentSum')]");
            status.CurrentBid = int.Parse(bid.InnerText, NumberStyles.AllowThousands | NumberStyles.Integer, client.ServerCulture);

            var count = content.SelectSingleNode("./div[contains(@class, 'numberOfBids')]");
            status.BidCount = int.Parse(count.InnerText, NumberStyles.Integer);

            var bidder = content.SelectSingleNode("./a[contains(@class, 'currentPlayer')]");
            status.HighestBidderName = bidder.InnerText.Trim();
            status.HighestBidderId = int.Parse(bidder.Attributes["data-player-id"]?.Value ?? "0");

            var item = content.SelectSingleNode(".//a[contains(@class, 'detail_button')]");

            string reference = item.Attributes["ref"].Value;

            IEnumerable<ShopItem> all = ShopItem.All();
            status.Item = all.Where(si => si.Reference == reference).FirstOrDefault();

            yield return status;
        }
    }
}
