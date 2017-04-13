using OgameBot.Engine.Parsing.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OgameBot.Engine.Commands
{
    public class FakePlanetExclusive : CommandBase, IPlanetExclusiveOperation
    {
        private DateTimeOffset _startedAt;

        public string Name => "Fake operation";

        public string Progress
        {
            get
            {
                double sec = ((DateTimeOffset.Now.Subtract(_startedAt).TotalSeconds / 30.0) * 100);

                return $"{sec:F1}%";
            }
        }

        public override void Run()
        {
            // start a long running demo on the current planet, prevent anyone else from changing it
            PlanetId = Client.IssueRequest(Client.RequestBuilder.GetPage(Objects.Types.PageType.Overview)).GetParsedSingle<OgamePageInfo>().PlanetId;
            using (Client.EnterPlanetExclusive(this))
            {
                _startedAt = DateTimeOffset.Now;
                Thread.Sleep(30000);
            }
        }
    }
}
