using HtmlAgilityPack;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Logging;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using OgameBot.Utilities;
using ScraperClientLib.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace OgameBot.Engine.Commands
{
    public class SendFleetCommand : CommandBase, IPlanetExclusiveOperation
    {
        private int _speed = 10;

        public Coordinate Source { get; set; }
        public Coordinate Destination { get; set; }
        public FleetComposition Fleet { get; set; }
        public MissionType Mission { get; set; }
        public int Speed
        {
            get { return _speed; }
            set
            {
                if (value < 1 || value > 10) throw new ArgumentOutOfRangeException("Speed must be between 1 and 10");
                _speed = value;
            }
        }

        private int _step;

        public int PlanetId { get; private set; }
        public string Name => $"Sending " + ToString();
        public string Progress => $"{_step} / 3";

        public SendFleetCommand(OGameClient client) : base(client)
        {
        }

        public override void Run()
        {
            using (BotDb db = new BotDb())
            {
                 PlanetId = (int)db.Planets.Where(s => s.LocationId == Source.Id).Select(s => s.PlanetId).First();
            }

            using (Client.EnterPlanetExclusive(this))
            {
                HttpRequestMessage req;
                ResponseContainer resp;
                Dictionary<string, string> postParams = new Dictionary<string, string>();


                req = Client.RequestBuilder.GetPage(PageType.Fleet, PlanetId);
                resp = Client.IssueRequest(req);


                // 1
                _step = 1;
                postParams = resp.GetHiddenFields();
                postParams.Merge(Fleet.Ships.ToDictionary(s => "am" + (int)s.Key, s => s.Value.ToString()));
                req = Client.RequestBuilder.PostPage(PageType.FleetDestination, postParams.ToArray());
                resp = Client.IssueRequest(req);

                // 2
                _step = 2;
                postParams = resp.GetHiddenFields();
                postParams["galaxy"] = Destination.Galaxy.ToString();
                postParams["system"] = Destination.System.ToString();
                postParams["position"] = Destination.Planet.ToString();
                postParams["type"] = ((int)Destination.Type).ToString();
                postParams["speed"] = Speed.ToString();
                req = Client.RequestBuilder.PostPage(PageType.FleetMission, postParams.ToArray());
                resp = Client.IssueRequest(req);


                // 3
                _step = 3;
                postParams = resp.GetHiddenFields();
                postParams["metal"] = Fleet.Resources.Metal.ToString();
                postParams["crystal"] = Fleet.Resources.Crystal.ToString();
                postParams["deuterium"] = Fleet.Resources.Deuterium.ToString();
                postParams["mission"] = ((int)Mission).ToString();

                req = Client.RequestBuilder.PostPage(PageType.FleetMovement, postParams.ToArray());
                resp = Client.IssueRequest(req);

                resp = Client.IssueRequest(Client.RequestBuilder.GetPage(PageType.FleetMovement));
                var fleets = resp.GetParsed<FleetInfo>();
                FleetInfo fi = fleets.Where(s => s.Origin.Coordinate == Source && s.Destination.Coordinate == Destination).FirstOrDefault();

                if (fi != null)
                {
                    Logger.Instance.Log(LogLevel.Success, $"Sent {this}");
                }
                else
                {
                    Logger.Instance.Log(LogLevel.Error, $"Could not send {this}");
                }
            }

        }

        public override string ToString()
        {
            return $"{Mission} Fleet from {Source} to {Destination}: {Fleet}";
        }


    }
}
