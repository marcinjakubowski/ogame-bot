using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects.Types;
using OgameBot.Utilities;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Parsing;
using OgameBot.Logging;

namespace OgameBot.Engine.Parsing
{
    public class FleetPageParser : BaseParser
    {
        private static Regex FleetsRegex = new Regex(@"([\d]+)/([\d]+)", RegexOptions.Compiled);

        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.Query.Contains("page=fleet1");
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            HtmlDocument doc = container.ResponseHtml.Value;
            HtmlNodeCollection listItemFields = doc.DocumentNode.SelectNodes("//div[@id='buttonz']/div[@class='content']//ul[@id='military' or @id='civil']/li");

            if (listItemFields == null)
                yield break;

            // 1 = fleets
            // 2 = expeditions
            // 3 = tactical retreat
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//div[@class='fleetStatus']//span[contains(@class, 'tooltip')]");
            if (nodes != null)
            {
                FleetSlotCount count = new FleetSlotCount();
                string fleetText = nodes[0].InnerText;
                Match match = FleetsRegex.Match(fleetText);
                if (match.Success)
                {
                    count.Current = int.Parse(match.Groups[1].Value);
                    count.Max = int.Parse(match.Groups[2].Value);

                    yield return count;
                }
                else
                {
                    Logger.Instance.Log(LogLevel.Error, $"Could not parse fleet count match: {fleetText}");
                }
            }



            foreach (HtmlNode node in listItemFields)
            {
                Ship type = ((ShipType)int.Parse(node.Id.Substring(6, 3)));
                string countText = node.SelectSingleNode(".//span[@class='level']").ChildNodes.Last(s => s.NodeType == HtmlNodeType.Text).InnerText;
                int count = int.Parse(countText, NumberStyles.Integer | NumberStyles.AllowThousands, client.ServerCulture);

                yield return new DetectedShip
                {
                    Ship = type,
                    Count = count
                };
            }
        }
    }
}