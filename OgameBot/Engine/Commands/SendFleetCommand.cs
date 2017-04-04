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
    public class SendFleetCommand : CommandBase
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

        public SendFleetCommand(OGameClient client) : base(client)
        {
        }

        public override void Run()
        {
            int planetId;
            using (BotDb db = new BotDb())
            {
                 planetId = (int)db.Planets.Where(s => s.LocationId == Source.Id).Select(s => s.PlanetId).First();
            }

            HttpRequestMessage req;
            ResponseContainer resp;
            Dictionary<string, string> postParams = new Dictionary<string, string>();


            req = Client.RequestBuilder.GetPage(PageType.Fleet, planetId);
            resp = Client.IssueRequest(req);


            // 1
            postParams = GetHiddenFields(resp.ResponseHtml.Value);
            postParams.Merge(Fleet.Ships.ToDictionary(s => "am" + (int)s.Key, s => s.Value.ToString()));
            req = Client.RequestBuilder.PostPage(PageType.FleetDestination, postParams.ToArray());
            resp = Client.IssueRequest(req);

            // 2
            postParams = GetHiddenFields(resp.ResponseHtml.Value);
            postParams["galaxy"] = Destination.Galaxy.ToString();
            postParams["system"] = Destination.System.ToString();
            postParams["position"] = Destination.Planet.ToString();
            postParams["type"] = ((int)Destination.Type).ToString();
            postParams["speed"] = Speed.ToString();
            req = Client.RequestBuilder.PostPage(PageType.FleetMission, postParams.ToArray());
            resp = Client.IssueRequest(req);


            // 3
            postParams = GetHiddenFields(resp.ResponseHtml.Value);
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
                StringBuilder sb = new StringBuilder();
                fi.Composition.Ships.ToList().ForEach(s => sb.AppendFormat("{1}x {0}, ", s.Key, s.Value));
                sb.Remove(sb.Length - 2, 2);
                Logger.Instance.Log(LogLevel.Success, $"Sent {Mission} fleet from {Source} to {Destination}: {sb.ToString()}");
            }
            else
            {
                Logger.Instance.Log(LogLevel.Error, $"Could not send {Mission} fleet from {Source} to {Destination}");
            }

        }

        private Dictionary<string, string> GetHiddenFields(HtmlDocument doc)
        {
            return doc.DocumentNode
                      .SelectNodes("//input[@type='hidden']")
                      .ToDictionary(s => s.GetAttributeValue("name", string.Empty),
                                    s => s.GetAttributeValue("value", string.Empty));
        }
    }
}
