using OgameBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameBot.Db
{
    public static class DbHelper
    {
        public static Coordinate GetPlanetCoordinateByCp(int cp)
        {
            using (BotDb db = new BotDb())
            {
                return db.Planets.Where(p => p.PlanetId == cp).Select(p => p.LocationId).FirstOrDefault();
            }
        }
    }
}
