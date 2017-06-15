using System.Text;
using ScraperClientLib.Engine;
using OgameBot.Engine.Parsing.Objects;
using System.Text.RegularExpressions;
using OgameBot.Proxy;

namespace OgameBot.Engine.Injects
{
    public class CommandsInject : IInject
    {
        private static Regex submenuRegex = new Regex(@"(title=""&lt.*cp=(\d+).*galaxy&amp;.*&gt;)", RegexOptions.Compiled);

        private int _port;
        private string _host;

        public string Inject(OgamePageInfo current, string body, ResponseContainer response, string host, int port)
        {
            if (current == null)
                return body;

            _port = port;
            _host = host;

            StringBuilder sb = new StringBuilder("$1");


            sb.Append("<hr/>");
            sb.Append(InjectHelper.GenerateCommandLink("farm?cp=$2", "Farm"))
                     .Append(" (")
                     .Append(InjectHelper.GenerateCommandLink("farm?cp=$2&slots=0", "0 slots"))
                     .Append(")")
                     .Append(NewLine);

            sb.Append(InjectHelper.GenerateCommandLink("hunt?cp=$2", "Hunt"))
                .Append(", range: ")
                .Append(InjectHelper.GenerateCommandLink("hunt?cp=$2&range=100", "100"))
                .Append(" ")
                .Append(InjectHelper.GenerateCommandLink("hunt?cp=$2&range=40", "40"))
                .Append(" ")
                .Append(InjectHelper.GenerateCommandLink("hunt?cp=$2&range=20", "20"))
                .Append(NewLine);

            sb.Append("Transport All: ")
                .Append(InjectHelper.GenerateCommandLink($"transport?from=$2&to={current.PlanetId}", "From"))
                .Append(" / ")
                .Append(InjectHelper.GenerateCommandLink($"transport?from={current.PlanetId}&to=$2", "To"))
                .Append(NewLine);

            sb.Append("Fleetsave: ")
                .Append(InjectHelper.GenerateCommandLink($"fs?cp=$2&in=360", "6h"))
                .Append(" ")
                .Append(InjectHelper.GenerateCommandLink($"fs?cp=$2&in=390", "6.5h"))
                .Append(" ")
                .Append(InjectHelper.GenerateCommandLink($"fs?cp=$2&in=420", "7h"))
                .Append(" ")
                .Append(InjectHelper.GenerateCommandLink($"fs?cp=$2&in=450", "7.5h"))
                .Append(" ")
                .Append(InjectHelper.GenerateCommandLink($"fs?cp=$2&in=480", "8h"))
                .Append(NewLine);


            sb.Append(InjectHelper.GenerateCommandLink("expedition?cp=$2", "Expedition"));

            body = submenuRegex.Replace(body, InjectHelper.EncodeTooltip(sb.ToString()));
            return body;
        }

        private const string NewLine = "</br>";
    }
}
