using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects.Types;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Parsing;

namespace OgameBot.Engine.Parsing
{
    public class JumpGateShipParser : BaseParser
    {
        private static readonly Regex ShipRegex = new Regex(@"toggleMaxShips\('#jumpgateForm', ([\d]+), ([\d]+)\)", RegexOptions.Compiled);

        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.Query.Contains("page=jumpgatelayer");
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            HtmlDocument doc = container.ResponseHtml.Value;

            HtmlNode token = doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='token']");
            if (token != null)
            {
                yield return new OgamePageInfo
                {
                    Page = PageType.JumpGate,
                    OrderToken = token.GetAttributeValue("value", string.Empty)
                };
            }

            HtmlNodeCollection ships = doc.DocumentNode.SelectNodes("//a[@class='dark_highlight_tablet' and @onclick]");
            // not ready
            if (ships == null) yield break;

            foreach (var ship in ships)
            {
                var shipTypeAndCount = ShipRegex.Match(ship.GetAttributeValue("onclick", ""));
                if (shipTypeAndCount.Success)
                {
                    yield return new DetectedShip
                    {
                        Ship = (ShipType) int.Parse(shipTypeAndCount.Groups[1].Value),
                        Count = int.Parse(shipTypeAndCount.Groups[2].Value)
                    };
                }

            }
        }
    }
}