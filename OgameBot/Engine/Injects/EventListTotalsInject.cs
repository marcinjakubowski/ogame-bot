using System;
using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Engine;
using OgameBot.Objects.Types;
using System.Linq;
using OgameBot.Objects;
using System.Collections.Generic;
using System.Text;

namespace OgameBot.Engine.Injects
{
    public class EventListTotalsInject : IInject
    {
        public string Inject(OgamePageInfo info, string body, ResponseContainer response, string host, int port)
        {
            if (!response.RequestMessage.RequestUri.PathAndQuery.Contains("page=eventList")) return body;

            var events = response.GetParsed<EventInfo>();


            var totalAttack = TotalFor(events.Where(ei => ei.IsReturning && ei.MissionType == MissionType.Attack));
            var totalTransport = TotalFor(events.Where(ei => !ei.IsReturning && (ei.MissionType == MissionType.Transport || ei.MissionType == MissionType.Deployment)));

            StringBuilder repl = new StringBuilder();


            string wrapper = "<tr class='eventFleet'><td colspan=11>Total cargo from {0}: <span class='textBeefy friendly'>{1}</span></td></tr>";

            if (totalAttack.Total > 0)
            {
                repl.Append(string.Format(wrapper, "attacks", totalAttack)); 
            }
            if (totalTransport.Total > 0)
            {
                repl.Append(string.Format(wrapper, "transport", totalTransport));
            }

            if (repl.Length > 0)
            {
                body = body.Replace(@"<tbody>", $@"<tbody>{repl.ToString()}");
            }

            return body;
        }

        private static Resources TotalFor(IEnumerable<EventInfo> events)
        {
            return events.Select(ei => ei.Composition.Resources)
                         .Aggregate(new Resources(), (x, y) => x + y);
        }
    }
}
