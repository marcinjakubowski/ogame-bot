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
        // #todo this works for commander only, is different when no commander available
        private static readonly Regex ActivityCountdownCommanderRegex = new Regex(@"new baulisteCountdown\(getElementByIdWithCache\(""(research)?Countdown""\),([\d]+)", RegexOptions.Compiled);

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
            HtmlNodeCollection scriptBlocks = doc.DocumentNode.SelectNodes("//script[@type='text/javascript' and not(@src)]");

            var construction = new DetectedOngoingConstruction();
            var research = new DetectedOngoingResearch();

            if (scriptBlocks != null)
            {
                foreach (HtmlNode block in scriptBlocks)
                {
                    var matches = ActivityCountdownCommanderRegex.Matches(block.InnerText);
                    if (matches.Count > 0)
                    {
                        Logger.Instance.Log(LogLevel.Error, $"Got {matches.Count} matches");

                        foreach (Match match in matches)
                        {
                            string type = match.Groups[1].Value;
                            int remainingDuration = int.Parse(match.Groups[2].Value);
                            DateTime finishingAt = DateTime.Now.AddSeconds(remainingDuration);

                            if (type == "research")
                            {
                                research.FinishingAt = finishingAt;
                            }
                            else
                            {
                                construction.FinishingAt = finishingAt;
                            }
                        }
                        break;
                    }
                }
            }
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//a[contains(@class, 'abortNow')]");
            if (nodes != null)
            {
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
                            construction.Building = (BuildingType)id;
                            construction.Level = level;
                            yield return construction;
                        }
                        else if (id >= 100 && id <= 200)
                        {
                            research.Research = (ResearchType)id;
                            research.Level = level;
                            yield return research;
                        }
                    }
                }

                
            }
        }
    }
}
