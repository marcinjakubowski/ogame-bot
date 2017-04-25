using System;
using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Engine;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace OgameBot.Engine.Injects
{

    public class CustomPlanetOrderInject : IInject
    {
        private static Regex planetListElement = new Regex(@"(<div class=""smallplanet.*?id=""planet-(\d+)"".*?</a>\s+</div>)", RegexOptions.Singleline | RegexOptions.Compiled);
        private IList<int> _planetOrder;

        public CustomPlanetOrderInject(IList<int> planetOrder)
        {
            _planetOrder = planetOrder;
        }

        public string Inject(OgamePageInfo info, string body, ResponseContainer response, string host, int port)
        {
            var matches = planetListElement.Matches(body);
            if (matches.Count == 0) return body;

            Dictionary<int, string> caps = new Dictionary<int, string>();
            int firstIndex = -1;
            foreach (Match m in matches)
            {
                int cp = int.Parse(m.Groups[2].Value);
                if (firstIndex == -1) firstIndex = m.Index;
                caps[cp] = m.Groups[1].Value;
            }
            body = planetListElement.Replace(body, "");

            StringBuilder sb = new StringBuilder();
            foreach (int cp in _planetOrder)
            {
                sb.Append(caps[cp]);
                caps.Remove(cp);
            }
            // Add any that aren't in the custom order setting

            foreach (var element in caps.Values)
            {
                sb.Append(element);
            }

            body = body.Insert(firstIndex, sb.ToString());

            

            return body;
        }
    }
}
