using System;
using System.Collections.Generic;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Parsing;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using OgameBot.Logging;
using OgameBot.Objects.Types;
using OgameBot.Engine.Parsing.Objects;

namespace OgameBot.Engine.Parsing
{
    public class OngoingActivityParser : BaseParser
    {
        private static readonly Regex ActivityIdRegex = new Regex(@"cancel((?:Production|Research))\(([\d]+).*level ([\d]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.Query.Contains("page=overview") ||
                container.RequestMessage.RequestUri.Query.Contains("page=resources") ||
                container.RequestMessage.RequestUri.Query.Contains("page=station") ||
                container.RequestMessage.RequestUri.Query.Contains("page=research");

        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            HtmlDocument doc = container.ResponseHtml.Value;
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[contains(@class, 'abortNow')]");

            foreach (HtmlNode node in nodes)
            {
                string onClick = node.GetAttributeValue("onclick", "");
                var match = ActivityIdRegex.Match(onClick);
                if (match.Success)
                {
                    int id = int.Parse(match.Groups[2].Value);
                    int level = int.Parse(match.Groups[3].Value);
                    // buildings
                    if (id < 100)
                    {
                        yield return new DetectedOngoingConstruction()
                        {
                            Building = (BuildingType)id,
                            Level = level
                        };
                    }
                    else if (id >= 100 && id <= 200)
                    {
                        yield return new DetectedOngoingResearch()
                        {
                            Research = (ResearchType)id,
                            Level = level
                        };
                    }
                }

                
            }
        }
    }
}
