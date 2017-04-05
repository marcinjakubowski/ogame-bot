using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScraperClientLib.Engine;
using OgameBot.Engine.Parsing.Objects;

namespace OgameBot.Engine.Injects
{
    public class CommandsInject : IInject
    {
        public string Inject(string body, ResponseContainer response)
        {
            int? cp = response.GetParsedSingle<OgamePageInfo>(false)?.PlanetId;
            if (cp.HasValue) body = body.Replace("/game/index.php?page=logout", $"/ogbcmd/farm?cp={cp}");

            return body;
        }
    }
}
