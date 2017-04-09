using OgameBot.Db;
using OgameBot.Engine.Commands;
using OgameBot.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OgameBot.Engine.Tasks
{
    public class Builder : WorkerBase
    {
        private readonly OGameClient _client;
        public Builder(OGameClient client) : base()
        {
            _client = client;
            ExecutionInterval = TimeSpan.FromSeconds(30);
        }

        // testing, 1-2-3
        protected override void RunInternal()
        {
            using (BotDb db = new BotDb())
            {
                //foreach ()
            }

        }


        public void Add(Planet planet, BuildingType building, int level)
        {
            Add(planet.LocationId, building, level);
        }

        public void Add(long locationId, BuildingType building, int level)
        {
            using (BotDb db = new BotDb())
            {
                BuildOrder order = new BuildOrder()
                {
                    LocationId = locationId,
                    Building = building,
                    Level = level
                };

                db.BuildOrder.Add(order);
                db.SaveChanges();
            }
        }
    }
}
