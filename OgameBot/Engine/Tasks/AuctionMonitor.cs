using System;
using OgameBot.Engine.Injects;
using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Engine;
using OgameBot.Utilities;
using System.Text.RegularExpressions;

namespace OgameBot.Engine.Tasks
{
    public class AuctionMonitor : WorkerBase, IInject
    {
        private OGameClient _client;
        private bool _selfIssued = false;
        private DateTimeOffset _lastRequest;

        private Regex clockRegex = new Regex(@"(<li class=""OGameClock.*</li>)", RegexOptions.Compiled);
        private AuctionStatus _current;

        public AuctionMonitor(OGameClient client)
        {
            _client = client;
            _client.OnResponseReceived += OnResponseReceived;
            ExecutionInterval = TimeSpan.FromMinutes(2);
        }

        private void OnResponseReceived(ResponseContainer response)
        {
            string path = response.RequestMessage.RequestUri.AbsolutePath;
            if (!_selfIssued && (path.Contains("page=traderOverview") || path.Contains("page=auctioneer")))
            {
                _lastRequest = DateTimeOffset.UtcNow;
            }
            ParseResponse(response);
        }

        private void ParseResponse(ResponseContainer response)
        {
            AuctionStatus status = response.GetParsedSingle<AuctionStatus>(false);
            if (status == null) return;

            _current = status;

        }

        public string Inject(OgamePageInfo info, string body, ResponseContainer response, string host, int port)
        {
            if (info?.Page == null) return body;


            string color;

            if ((_current?.MinutesRemaining ?? 0) == 0) return body;
            else if (_current.MinutesRemaining >= 20) color = "color:#99CC00;";
            else if (_current.MinutesRemaining >= 10) color = "color:#ffa500";
            else if (_current.MinutesRemaining == 5) color = "color:#ff0000";
            else color = string.Empty;

            body = clockRegex.Replace(body, $"$1<li style='float:right'><a href='/game/index.php?page=traderOverview#animation=false&page=traderAuctioneer' class='tooltip' title='{_current.Item} @ {_current.CurrentBid}'>Auctioneer: <span style='{color};font-weight:bold;'>{_current.MinutesRemaining}m</span></a></li>");
            return body;
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
