using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Parsing;

namespace OgameBot.Engine.Parsing
{
    public class PlanetListParser : BaseParser
    {
        private static readonly Regex PlanetIdRegex = new Regex(@"planet-([\d]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex MoonIdRegex = new Regex(@"cp=([\d]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            // Always parse this (if it's html)
            return container.IsHtmlResponse;
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            HtmlDocument doc = container.ResponseHtml.Value;
            HtmlNodeCollection worldNodes = doc.DocumentNode.SelectNodes("//div[@id='planetList']/div[starts-with(@id, 'planet-')]");

            if (worldNodes == null)
                yield break;

            foreach (HtmlNode node in worldNodes)
            {
                int id = int.Parse(PlanetIdRegex.Match(node.GetAttributeValue("id", null)).Groups[1].Value, client.ServerCulture);
                string name = node.SelectSingleNode(".//span[contains(@class, 'planet-name')]").InnerText;
                string coordinate = node.SelectSingleNode(".//span[contains(@class, 'planet-koords')]").InnerText;

                PlanetListItem item = new PlanetListItem
                {
                    Id = id,
                    Name = name,
                    Coordinate = Coordinate.Parse(coordinate, CoordinateType.Planet)
                };

                yield return item;

                var moon = node.SelectSingleNode(".//a[contains(@class, 'moonlink')]");
                if (moon != null)
                {
                    int moonId = int.Parse(MoonIdRegex.Match(moon.GetAttributeValue("href", string.Empty)).Groups[1].Value, client.ServerCulture);
                    string moonName = moon.SelectSingleNode("./img").GetAttributeValue("alt", string.Empty);

                    yield return new PlanetListItem
                    {
                        Id = moonId,
                        Name = moonName,
                        Coordinate = Coordinate.Parse(coordinate, CoordinateType.Moon)
                    };
                }
            }

        }
    }
}