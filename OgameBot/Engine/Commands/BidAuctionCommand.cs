using System;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Utilities;
using OgameBot.Logging;
using System.Threading;
using OgameBot.Objects.Types;

namespace OgameBot.Engine.Commands
{
    public class BidAuctionCommand : CommandBase
    {
        public ResourceType BidResource { get; set; } = ResourceType.Deuterium;

        protected override CommandQueueElement RunInternal()
        {
            var resp = Client.IssueRequest(Client.RequestBuilder.GetOverviewPage(PlanetId));
            var info = resp.GetParsedSingle<OgamePageInfo>();

            int bidCount = 0, checkCount = 0;

            double multiplier = 1;

            switch (BidResource)
            {
                case ResourceType.Metal:
                    multiplier = 1.0;
                    break;
                case ResourceType.Crystal:
                    multiplier = 1.5;
                    break;
                case ResourceType.Deuterium:
                    multiplier = 3.0;
                    break;
            }


            AuctionStatus status = null;
            AuctionBidResponse last = null;
            do
            {
                resp = Client.IssueRequest(Client.RequestBuilder.GetAuctioneer());
                status = resp.GetParsedSingle<AuctionStatus>();
                checkCount++;
                if (status.HighestBidderId != info.PlayerId && status.MinutesRemaining != 0)
                {
                    int bidValue = (int)Math.Ceiling(status.Shortfall / multiplier);

                    var req = Client.BuildPost(new Uri("/game/index.php?page=auctioneer", UriKind.Relative), new[]
                    {
                        KeyValuePair.Create("ajax", "1"),
                        KeyValuePair.Create($"bid[planets][{PlanetId}][metal]", BidResource == ResourceType.Metal ? bidValue.ToString() : "0"),
                        KeyValuePair.Create($"bid[planets][{PlanetId}][crystal]", BidResource == ResourceType.Crystal ? bidValue.ToString() : "0"),
                        KeyValuePair.Create($"bid[planets][{PlanetId}][deuterium]", BidResource == ResourceType.Deuterium ? bidValue.ToString() : "0"),
                        KeyValuePair.Create($"bid[honor]", "0"),
                        KeyValuePair.Create("token", status.Token)
                    });

                    var bid = Client.IssueRequest(req);

                    last = bid.GetParsedSingle<AuctionBidResponse>(false);
                    Logger.Instance.Log(LogLevel.Warning, $"Bid {bidCount++}/{checkCount}: {last?.Message}");
                }
                if (!last.Error) Thread.Sleep(1000);
            } while (status.MinutesRemaining != 0);

            return null;
        }
    }
}
