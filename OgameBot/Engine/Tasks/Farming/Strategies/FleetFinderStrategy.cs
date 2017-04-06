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
        public int ProbeCount { get; set; } = 5;

        public IEnumerable<Planet> GetFarms(SystemCoordinate from, SystemCoordinate to)
        {
            using (BotDb db = new BotDb())
            {
                var farms = db.Planets.Where(s =>
                                        s.LocationId >= from.Id && s.LocationId <= to.Id
                                     && !((s.Player.Status.HasFlag(PlayerStatus.Inactive) || s.Player.Status.HasFlag(PlayerStatus.LongInactive)))
                                     && !s.Player.Status.HasFlag(PlayerStatus.Vacation)
                                     && !s.Player.Status.HasFlag(PlayerStatus.Admin)
                                     && !s.Player.Status.HasFlag(PlayerStatus.Noob)
                                     && (!s.Player.Status.HasFlag(PlayerStatus.Strong) || s.Player.Status.HasFlag(PlayerStatus.Outlaw))
                                ).ToList();

                return farms;
            }
        }

        public int GetProbeCountForTarget(Planet target)
        {
            return ProbeCount;
        }

        public IEnumerable<AttackTarget> GetTargets(IEnumerable<EspionageReport> reports)
        {
            // We don't attack here, just gather information
            yield break;
        }

        public void OnAfterAttack()
        {
        }

        public void OnBeforeAttack()
        {
        }
    }
}
