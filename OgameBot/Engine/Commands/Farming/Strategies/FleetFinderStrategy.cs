using System.Collections.Generic;
using System.Linq;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects;
using OgameBot.Db.Parts;
using OgameBot.Objects.Types;
using System.Threading;

namespace OgameBot.Engine.Commands.Farming.Strategies
{
    public class FleetFinderStrategy : IFarmingStrategy
    {
        public int EspionageTechnologyLevel { get; set; } = 8;
        public int MaxRanking { get; set; } = 400;
        public int MinValue { get; set; } = 500000;
        public int ProbeCount { get; set; } = 2;

        private OGameClient _client => OGameClient.Instance;

        public IEnumerable<Planet> GetFarms(SystemCoordinate from, SystemCoordinate to)
        {
            using (BotDb db = new BotDb())
            {
                var farms = db.Planets.Where(s =>
                                        s.LocationId >= from.LowerCoordinate && s.LocationId <= to.UpperCoordinate
                                     && !((s.Player.Status.HasFlag(PlayerStatus.Inactive) || s.Player.Status.HasFlag(PlayerStatus.LongInactive)))
                                     && !s.Player.Status.HasFlag(PlayerStatus.Vacation)
                                     && !s.Player.Status.HasFlag(PlayerStatus.Admin)
                                     && !s.Player.Status.HasFlag(PlayerStatus.Noob)
                                     && (!s.Player.Status.HasFlag(PlayerStatus.Strong) || s.Player.Status.HasFlag(PlayerStatus.Outlaw))
                                     && (s.Player.Ranking > MaxRanking)
                                     // Either it's the first time we're scanning this target, and we've never learned its technologies
                                     // or their espionage technology is no higher than one than our own                                     
                                     //&& (s.LastResourcesTime == null || (s.Player.Research.EspionageTechnology ?? 99) <= EspionageTechnologyLevel + 1)
                                ).ToList();

                return farms;
            }
        }
        /* Formula: ProbeCount + (ET_Self - ET_Target) * ABS(ET_Self - ET_Target)
         * <2	Resources
         *  2	Resources + Fleet
         *  3	Resources + Fleet + Defense
         *  5	Resources + Fleet + Defense + Buildings
         *  7	Resources + Fleet + Defense + Buildings + Research
        */

        public IEnumerable<Target> GetTargets(IEnumerable<EspionageReport> reports)
        {
            // Has ships but not defense, just one more probe away
            return reports.Where(r => r.Details.HasFlag(ReportDetails.Ships) && !r.Details.HasFlag(ReportDetails.Defense) && ((PlanetShips)r.DetectedShips).TotalValue >= MinValue)
                          .Select(r => new Target
                          {
                              Destination = r.Coordinate,
                              Fleet = FleetComposition.ToSpy(ProbeCount + 1),
                              Mission = MissionType.Espionage
                          });
        }

        public void OnAfterAttack()
        {
            // Wait one minute for the probes to come back
            // #todo extract method from FarmingBot
            Thread.Sleep(60000);
            ReadAllMessagesCommand cmd = new ReadAllMessagesCommand();
            cmd.Run();
        }

        public bool OnBeforeAttack()
        {
            return true;
        }
    }
}
