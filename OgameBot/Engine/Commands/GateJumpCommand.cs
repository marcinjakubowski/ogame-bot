using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using OgameBot.Utilities;
using ScraperClientLib.Engine;

namespace OgameBot.Engine.Commands
{
    public class GateJumpCommand : CommandBase, IPlanetExclusiveOperation
    {
        public int Destination { get; set; }
        public FleetComposition Fleet { get; set; }

        protected override CommandQueueElement RunInternal()
        {
            using (Client.EnterPlanetExclusive(this))
            {
                HttpRequestMessage req;
                ResponseContainer resp;

                req = Client.RequestBuilder.GetPage(PageType.JumpGate, PlanetId);
                resp = Client.IssueRequest(req);

                if (Fleet == null)
                {
                    Fleet = FleetComposition.FromDetected(resp.GetParsed<DetectedShip>());
                }

                Dictionary<string, string> postParams = new Dictionary<string, string>();
                postParams["token"] = resp.GetParsedSingle<OgamePageInfo>().OrderToken;
                postParams["zm"] = Destination.ToString();
                postParams.Merge(Fleet.Ships.ToDictionary(s => "ship_" + (int)s.Key, s => s.Value.ToString()));

                req = Client.RequestBuilder.PostPage(PageType.JumpGateExecute, postParams.ToArray());
                resp = Client.IssueRequest(req);
            }

            return null;
        }

        public string Name => "Gate jump";
        public string Progress => "Jumping";
    }
}