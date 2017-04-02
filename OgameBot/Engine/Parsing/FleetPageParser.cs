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

namespace OgameBot.Engine.Parsing
{
    public class FleetPageParser : BaseParser
    {
        private static Regex CssRegex = new Regex(@"(?:military[\d]+|civil[\d]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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