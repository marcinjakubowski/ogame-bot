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
        public TimeSpan AcceptableWindow { get; set; } = TimeSpan.FromMinutes(5);

        public string Name => "Fleet save";
        public string Progress { get; }

        private TimeSpan _oneWayTrip;

        protected override CommandQueueElement RunInternal()
        {
            _oneWayTrip = TimeSpan.FromSeconds((ReturnTime - DateTimeOffset.Now).TotalSeconds / 2);

            using (BotDb db = new BotDb())
            using (Client.EnterPlanetExclusive(this))
            {
                var resp = Client.IssueRequest(Client.RequestBuilder.GetPage(PageType.Fleet, PlanetId));
                var info = resp.GetParsedSingle<OgamePageInfo>();

                FleetComposition fleet = FleetComposition.FromDetected(resp.GetParsed<DetectedShip>());
                if (!fleet.Ships.ContainsKey(ShipType.Recycler))
                {
                    Logger.Instance.Log(LogLevel.Error, "Planet does not have a recycler, cannot fleetsave");
                    return null;
                }
                fleet.Resources = resp.GetParsedSingle<PlanetResources>().Resources;

                PlayerResearch research = db.Players.Where(p => p.PlayerId == info.PlayerId).Select(p => p.Research).First();
                int fleetSpeed = fleet.Speed(research);

                Coordinate here = info.PlanetCoord;
                FleetSaveTarget candidate = CandidateFromSystem(db, here, fleetSpeed) ?? CandidateFromGalaxy(db, here, fleetSpeed);

                if (candidate != null)
                {
                    Logger.Instance.Log(LogLevel.Success, $"Best candidate for fleetsave is {candidate.Target} at speed {candidate.Speed}; one way trip {candidate.Duration}");

                    new SendFleetCommand()
                    {
                        PlanetId = PlanetId,
                        Destination = Coordinate.Create(candidate.Target, CoordinateType.DebrisField),
                        Speed = candidate.Speed,
                        Mission = MissionType.Recycle,
                        Fleet = fleet
                    }.Run();
                }
                else
                {
                    Logger.Instance.Log(LogLevel.Error, $"Could not find a good candidate for fleetsave");
                }
            }
            return null;
        }

        private FleetSaveTarget CandidateFromGalaxy(BotDb db, Coordinate here, int fleetSpeed)
        {
            SystemCoordinate hereSystem = here;
            FleetSaveTarget best = null;
            for (int speed = 1; speed <= 10 && best == null; speed++)
            {
                for (short range = 1; range <= 100 && best == null; range++)
                {
                    short systemCoordLeft = (short)(hereSystem.System - range);
                    short systemCoordRight = (short)(hereSystem.System + range);
                    if (systemCoordLeft <= 0) systemCoordLeft = 0;
                    else if (systemCoordRight >= 499) systemCoordRight = systemCoordLeft;

                    SystemCoordinate systemLeft = new SystemCoordinate(hereSystem.Galaxy, systemCoordLeft);
                    SystemCoordinate systemRight = new SystemCoordinate(hereSystem.Galaxy, systemCoordRight);

                    var duration = here.DurationTo(systemRight.LowerCoordinate, fleetSpeed, speed, OGameClient.Instance.Settings.Speed);

                    if (GetDifference(duration, _oneWayTrip) > AcceptableWindow)
                        continue;

                    var targets = db.Planets
                                    .Where(p => (p.LocationId >= systemLeft.LowerCoordinate && p.LocationId <= systemLeft.UpperCoordinate)
                                             || (p.LocationId >= systemRight.LowerCoordinate && p.LocationId <= systemRight.UpperCoordinate))
                                    .Select(p => p.LocationId)
                                    .AsEnumerable()
                                    .Select(target => new FleetSaveTarget { Target = target, Duration = duration, Speed = speed });

                    best = FilterDebrisAvailable(targets).FirstOrDefault();
                }
            }

            return best;
        }

        private FleetSaveTarget CandidateFromSystem(BotDb db, Coordinate here, int fleetSpeed)
        {
            SystemCoordinate hereSystem = here;
            Coordinate herePlanet = Coordinate.Create(here, CoordinateType.Planet);
            Coordinate hereMoon = Coordinate.Create(here, CoordinateType.Moon);
            var currentSystemPlanets = db.Planets.Where(p => p.LocationId >= hereSystem.LowerCoordinate && p.LocationId <= hereSystem.UpperCoordinate
                                                          && p.LocationId != herePlanet.Id
                                                          && p.LocationId != hereMoon.Id)
                                                 .Select(p => p.LocationId)
                                                 .AsEnumerable();

            IEnumerable<FleetSaveTarget> candidates = currentSystemPlanets.Cartesian(Enumerable.Range(1, 10), (target, speed) => new FleetSaveTarget
            {
                Target = target,
                Duration = here.DurationTo(target, fleetSpeed, speed, Client.Settings.Speed),
                Speed = speed
            });

            return FilterDebrisAvailable(FilterTime(candidates)).FirstOrDefault();
        }

        private IEnumerable<FleetSaveTarget> FilterTime(IEnumerable<FleetSaveTarget> candidates)
        {
            return candidates.Where(c => GetDifference(c.Duration, _oneWayTrip) <= AcceptableWindow);
        }

        private IEnumerable<FleetSaveTarget> FilterDebrisAvailable(IEnumerable<FleetSaveTarget> candidates)
        {
            return candidates.Where(c => CheckDebris(c));
        }

        private bool CheckDebris(FleetSaveTarget candidate)
        {
            var resp = Client.IssueRequest(Client.RequestBuilder.GetFleetCheckDebris(candidate.Target));
            return resp.GetParsedSingle<FleetCheck>().Status == FleetCheckStatus.OK;
        }

        private static TimeSpan GetDifference(TimeSpan current, TimeSpan desired)
        {
            TimeSpan ts = current - desired;

            if (ts.Ticks < 0) return TimeSpan.FromDays(10);

            return ts;
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
