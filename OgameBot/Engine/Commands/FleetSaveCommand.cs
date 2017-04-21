using System;
using System.Collections.Generic;
using System.Linq;
using OgameBot.Db;
using OgameBot.Objects.Types;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Db.Parts;
using OgameBot.Objects;
using MoreLinq;
using OgameBot.Logging;

namespace OgameBot.Engine.Commands
{
    public class FleetSaveCommand : CommandBase, IPlanetExclusiveOperation
    {
        public DateTimeOffset ReturnTime { get; set; }

        public string Name => "Fleet save";
        public string Progress { get; }

        protected override CommandQueueElement RunInternal()
        {
            using (BotDb db = new BotDb())
            using (Client.EnterPlanetExclusive(this))
            {
                var resp = Client.IssueRequest(Client.RequestBuilder.GetPage(PageType.Fleet, PlanetId));
                var info = resp.GetParsedSingle<OgamePageInfo>();

                PlayerResearch research = db.Players.Where(p => p.PlayerId == info.PlayerId).Select(p => p.Research).First();
                FleetComposition fleet = FleetComposition.FromDetected(resp.GetParsed<DetectedShip>());
                fleet.Resources = resp.GetParsedSingle<PlanetResources>().Resources;

                Coordinate here = info.PlanetCoord;
                TimeSpan oneWayTrip = TimeSpan.FromSeconds((ReturnTime - DateTimeOffset.Now).TotalSeconds / 2);

                List<FleetSaveTarget> targets = new List<FleetSaveTarget>();

                SystemCoordinate currentSystem = here;
                var currentSystemPlanets = db.Planets.Where(p => p.LocationId >= currentSystem.LowerCoordinate && p.LocationId <= currentSystem.UpperCoordinate && p.LocationId != here.Id && p.LocationId != here.Id - 2)
                                                     .Select(p => p.LocationId)
                                                     .ToList();

                int fleetSpeed = fleet.Speed(research);

                var candidates = currentSystemPlanets.Cartesian(Enumerable.Range(1, 10), (target, speed) => new FleetSaveTarget { Target = target, Duration = here.DurationTo(target, fleetSpeed, speed, Client.Settings.Speed), Speed = speed });
                var candidate = candidates.MinBy(c => (c.Duration - oneWayTrip).Duration());
                targets.Add(candidate);


                //Logger.Instance.Log(LogLevel.Warning, $"Best candidate for fleetsave would be {bestCandidate.Key}, would be there home in {bestCandidate.Value}");

            }
            return null;
        }

        private class FleetSaveTarget
        {
            public Coordinate Target { get; set; }
            public int Speed { get; set; }
            public TimeSpan Duration { get; set; }

            public override string ToString()
            {
                return $"{Target} x{Speed} => {Duration}";
            }
        }
    }
}
