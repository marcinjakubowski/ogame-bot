using System;
using OgameBot.Engine.Injects;
using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Engine;
using OgameBot.Utilities;
using System.Text.RegularExpressions;

namespace OgameBot.Engine.Tasks
{
    public class AuctionMonitor : WorkerBase
    {
        private OGameClient _client;
        private bool _selfIssued = false;
        private DateTimeOffset _lastRequest;
        
        public AuctionStatus Auction { get; private set; }

        public AuctionMonitor(OGameClient client)
        {
            _client = client;
            _client.OnResponseReceived += OnResponseReceived;
            ExecutionInterval = TimeSpan.FromMinutes(2);
        }

        private void OnResponseReceived(ResponseContainer response)
        {
            string path = response.RequestMessage.RequestUri.AbsoluteUri;
            if (path.Contains("page=traderOverview") || path.Contains("page=auctioneer"))
            {
                if (!_selfIssued)
                {
                    _lastRequest = DateTimeOffset.UtcNow;
                }
                ParseResponse(response);
            }
        }

        private void ParseResponse(ResponseContainer response)
        {
            AuctionStatus status = response.GetParsedSingle<AuctionStatus>(false);
            switch (status?.MinutesRemaining ?? 10)
            {
                case 0:
                    ExecutionInterval = status.NextIn.Add(TimeSpan.FromMinutes(1));
                    break;
                case 5:
                    ExecutionInterval = TimeSpan.FromMinutes(1);
                    break;
                default:
                    ExecutionInterval = TimeSpan.FromMinutes(2);
                    break;
            }
            if (status != null)
                Auction = status;
            Restart();
        }

        protected override void RunInternal()
        {
            if (DateTimeOffset.UtcNow - _lastRequest < ExecutionInterval) return;
            var req = _client.RequestBuilder.GetAuctioneer();

            _selfIssued = true;
            var resp = _client.IssueRequest(req);
            _selfIssued = false;
        }
    }
}
