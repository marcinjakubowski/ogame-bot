using System;
using System.Collections.Generic;
using System.Linq;
using ScraperClientLib.Engine.Parsing;
using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Utilities;

namespace OgameBot.Engine.Savers
{
    public class GalaxyActivitySaver : SaverBase
    {
        public override void Run(List<DataObject> result)
        {
            using (BotDb db = new BotDb())
            {
                var logs = FromGalaxy(result).ToList();

                var newLogs = logs.Where(log => !db.PlanetActivityLog.Where(existing => existing.LocationId == log.LocationId && existing.CreatedOn == log.CreatedOn).Any());
                db.PlanetActivityLog.AddRange(newLogs);
                db.SaveChanges();
            }
        }

        private IEnumerable<PlanetActivityLog> FromGalaxy(List<DataObject> result)
        {
            foreach (var row in result.OfType<GalaxyPageInfoItem>())
            {
                yield return FromGalaxyPartItem(row.Planet);

                if (row.Moon != null)
                {
                    yield return FromGalaxyPartItem(row.Moon);
                }
            }
        }

        private PlanetActivityLog FromGalaxyPartItem(GalaxyPageInfoPartItem galaxyItem)
        {
            return new PlanetActivityLog()
            {
                LocationId = galaxyItem.Coordinate.Id,
                Activity = galaxyItem.Activity,
                CreatedOn = DateTimeOffset.Now.TruncateToMinute()
            };
        }
    }
}
