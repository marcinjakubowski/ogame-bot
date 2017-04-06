using System.Text;
using ScraperClientLib.Engine;
using OgameBot.Engine.Parsing.Objects;
using System.Text.RegularExpressions;

namespace OgameBot.Engine.Injects
{
    public class CommandsInject : IInject
    {
        private static Regex galaxysubmenuRegex = new Regex(@"(title=""&lt.*cp=(\d+).*&gt;)", RegexOptions.Compiled);

        public string Inject(OgamePageInfo current, string body, ResponseContainer response)
        {
            if (current == null)
                return body;

            string[][] links = new string[][] {
                new string[] {"farm?cp=$2", "Farm"},
                new string[] {"hunt?cp=$2", "Hunt Fleets"},
                new string[] {$"transport?from=$2&to={current.PlanetId}", "Transport All From"},
                new string[] {$"transport?from={current.PlanetId}&to=$2", "Transport All To"}
            };

            StringBuilder sb = new StringBuilder("$1&lt;hr/&gt;");
            foreach (string[] link in links)
            {
                sb.Append($"&lt;a href=&quot;http://127.0.0.1:18000/ogbcmd/{link[0]}&quot;&gt;{link[1]}&lt;/a&gt;&lt;/br&gt;");
            }
            sb.Length -= 11;
            body = galaxysubmenuRegex.Replace(body, sb.ToString());
            return body;
        }
    }
}
