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
        private TimeSpan _oneWayTrip;

        static readonly TimeSpan AcceptableWindow = TimeSpan.FromMinutes(5);

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
                _oneWayTrip = TimeSpan.FromSeconds((ReturnTime - DateTimeOffset.Now).TotalSeconds / 2);

                SystemCoordinate currentSystem = here;
                var currentSystemPlanets = db.Planets.Where(p => p.LocationId >= currentSystem.LowerCoordinate && p.LocationId <= currentSystem.UpperCoordinate && p.LocationId != here.Id && p.LocationId != here.Id - 2)
                                                     .Select(p => p.LocationId)
                                                     .ToList();

                int fleetSpeed = fleet.Speed(research);

                var local = currentSystemPlanets.Cartesian(Enumerable.Range(1, 10), (target, speed) => new FleetSaveTarget { Target = target, Duration = here.DurationTo(target, fleetSpeed, speed, Client.Settings.Speed), Speed = speed });

                FleetSaveTarget best = null;

                if (CheckTargets(local))
                {
                    best = local.Where(fs => fs.Valid).MinBy(fs => fs.GetDifference(_oneWayTrip));
                }
                else
                {
                    for (int speed = 1; speed <= 10 && best == null; speed++)
                    {
                        for (short r = 1; r < 100; r++)
                        {
                            short systemLeft = (short)(currentSystem.System - r);
                            short systemRight = (short)(currentSystem.System + r);
                            if (systemLeft <= 0) systemLeft = 0;
                            else if (systemRight >= 499) systemRight = systemLeft;

                            SystemCoordinate sysLeft = new SystemCoordinate(currentSystem.Galaxy, systemLeft);
                            SystemCoordinate sysRight = new SystemCoordinate(currentSystem.Galaxy, systemRight);

                            SystemCoordinate check = systemLeft != 0 ? sysLeft : sysRight;

                            var duration = here.DurationTo(check.LowerCoordinate, fleetSpeed, speed, OGameClient.Instance.Settings.Speed);

                            if ((duration - _oneWayTrip).Duration() < AcceptableWindow)
                            {
                                var targets = db.Planets.Where(p => (p.LocationId >= sysLeft.LowerCoordinate && p.LocationId <= sysLeft.UpperCoordinate) ||
                                                                    (p.LocationId >= sysRight.LowerCoordinate && p.LocationId <= sysRight.UpperCoordinate))
                                                        .Select(p => new FleetSaveTarget { Target = p.LocationId, Duration = duration, Speed = speed });

                                if (CheckTargets(targets))
                                {
                                    best = local.Where(fs => fs.Valid).MinBy(fs => fs.GetDifference(_oneWayTrip));
                                    break;
                                }
                            }
                        }
                    }
                }

                Logger.Instance.Log(LogLevel.Warning, $"Best candidate for fleetsave would be {best.Target} at speed {best.Speed}; one way trip {best.Duration}");

            }
            return null;
        }

        private bool CheckTargets(IEnumerable<FleetSaveTarget> targets)
        {
            bool found = false;
            foreach (FleetSaveTarget target in targets)
            {
                if (target.GetDifference(_oneWayTrip) > AcceptableWindow) {
                    target.Valid = false;
                    continue;
                }
                var resp = Client.IssueRequest(Client.RequestBuilder.GetFleetCheckDebris(target.Target));
                target.Valid = resp.GetParsedSingle<FleetCheck>().Status == FleetCheckStatus.OK;
                if (target.Valid) found = true;
            }

            return found;
        }

        private class FleetSaveTarget
        {
            public Coordinate Target { get; set; }
            public int Speed { get; set; }
            public TimeSpan Duration { get; set; }

            public TimeSpan GetDifference(TimeSpan ts)
            {
                return (Duration - ts).Duration();
            }

            public bool Valid { get; set; } = false;

            public override string ToString()
            {
                return $"{Target} x{Speed} => {Duration}";
            }
        }
    }
}
