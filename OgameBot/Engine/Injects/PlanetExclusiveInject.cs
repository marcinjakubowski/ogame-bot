using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Engine;
using OgameBot.Db;
using System.Web;

namespace OgameBot.Engine.Injects
{
    public class PlanetExclusiveInject : IInject
    {
        private OGameClient _client;

        public PlanetExclusiveInject(OGameClient client)
        {
            _client = client;
        }
        public string Inject(OgamePageInfo info, string body, ResponseContainer response, string host, int port)
        {
            if (!response.RequestMessage.RequestUri.PathAndQuery.Contains("ogpe=1"))
                return body;


            using (BotDb db = new BotDb())
            {
                IPlanetExclusiveOperation op = _client.CurrentPlanetExclusiveOperation?.Operation;
                // Disposed since check was made
                if (op == null) return body;

                Planet planet = db.Planets.Where(p => p.PlanetId == op.PlanetId).First();

                // 
                body += $"<script type='text/javascript'>errorBoxNotify('Ongoing activity','There is an ongoing bot activity on planet {planet.Name} {planet.Coords} - {op.Name}: {op.Progress}! Cannot change planets right now.','OK');</script>";
                return body;
            }
        }
    }
}
