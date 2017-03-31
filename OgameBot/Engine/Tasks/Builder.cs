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
            ExecutionInterval = TimeSpan.FromSeconds(15);
        }

        // testing, 1-2-3
        protected override void RunInternal()
        {
            using (BotDb db = new BotDb())
            {
                /*
                Planet p = db.Planets.Where(s => s.Name == "<insert>").FirstOrDefault();
                BuildCommand cmd = new BuildCommand(_client, p, BuildingType.ResearchLab);
                cmd.Run();
                */
            }
            Stop();
        }
    }
}
