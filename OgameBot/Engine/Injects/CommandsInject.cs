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

            StringBuilder sb = new StringBuilder("$1&lt;hr/&gt;");
            
            sb.Append(GetLink("farm?cp=$2", "Farm")).Append(" (").Append(GetLink("farm?cp=$2&slots=0", "0 slots")).Append(")").Append(NewLine);
            sb.Append(GetLink("hunt?cp=$2", "Hunt")).Append(", range: ").Append(GetLink("hunt?cp=$2&range=40", "40")).Append(" ").Append(GetLink("hunt?cp=$2&range=20", "20")).Append(NewLine);
            sb.Append("Transport All: ").Append(GetLink($"transport?from=$2&to={current.PlanetId}", "From")).Append(" / ").Append(GetLink($"transport?from={current.PlanetId}&to=$2", "To"));
            body = galaxysubmenuRegex.Replace(body, sb.ToString());
            return body;
        }

        private static string GetLink(string command, string label)
        {
            return $"&lt;a href=&quot;http://127.0.0.1:18000/ogbcmd/{command}&quot;&gt;{label}&lt;/a&gt;";
        }

        private const string NewLine = "&lt;/br&gt;";
    }
}
