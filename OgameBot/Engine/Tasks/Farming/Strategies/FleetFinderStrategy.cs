using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects;

namespace OgameBot.Engine.Tasks.Farming.Strategies
{
    public class FleetFinderStrategy : IFarmingStrategy
    {
        public int EspionageTechnologyLevel { get; set; } = 8;
        public int MaxRanking { get; set; } = 400;

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
        public int GetProbeCountForTarget(Planet target)
        {
            // Lets you see defenses for targets with EspionageTechnology +1 than your own
            return 4;
        }

        public IEnumerable<Target> GetTargets(IEnumerable<EspionageReport> reports)
        {
            // We don't attack here, just gather information
            yield break;
        }

        public void OnAfterAttack()
        {
            // Schedule another farm bot for targets we have fleet for but not the defences
        }

        public void OnBeforeAttack()
        {
        }
    }
}
