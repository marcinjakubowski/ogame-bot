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

            int bidCount = 1, checkCount = 0;

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
                    int bidValue = status.Shortfall;

                    if (status.Shortfall < 3000) bidValue *= 2;
                    else if (status.Shortfall < 100000) bidValue = (int)(bidValue * 1.2);
                    else if (status.Shortfall > 200000)
                    {
                        Logger.Instance.Log(LogLevel.Error, $"Too high bid amount: {bidValue}, bid on your own or rerun!");
                        return null;
                    }
                    bidValue += 1;
                    int bid = (int)(Math.Ceiling(bidValue / multiplier));

                    var req = Client.BuildPost(new Uri("/game/index.php?page=auctioneer", UriKind.Relative), new[]
                    {
                        KeyValuePair.Create("ajax", "1"),
                        KeyValuePair.Create($"bid[planets][{PlanetId}][metal]", BidResource == ResourceType.Metal ? bid.ToString() : "0"),
                        KeyValuePair.Create($"bid[planets][{PlanetId}][crystal]", BidResource == ResourceType.Crystal ? bid.ToString() : "0"),
                        KeyValuePair.Create($"bid[planets][{PlanetId}][deuterium]", BidResource == ResourceType.Deuterium ? bid.ToString() : "0"),
                        KeyValuePair.Create($"bid[honor]", "0"),
                        KeyValuePair.Create("token", status.Token)
                    });

                    resp = Client.IssueRequest(req);
                    last = resp.GetParsedSingle<AuctionBidResponse>(false);

                    Logger.Instance.Log(LogLevel.Info, $"Bid {bidCount++}/{checkCount} was for {status.OwnBid + bidValue}: {last?.Message}");
                }

                int delay = 0 ;
                if (status.MinutesRemaining > 5) delay = 30000;
                Thread.Sleep(delay);

            } while (status.MinutesRemaining != 0);

            Logger.Instance.Log(status.HighestBidderId == info.PlayerId ? LogLevel.Success : LogLevel.Warning, $"Auction for {status.Item} won by {status.HighestBidderName} at {status.CurrentBid}");
            return null;
        }
    }
}
