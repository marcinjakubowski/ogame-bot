using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Engine.Parsing.UtilityParsers;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Parsing;

namespace OgameBot.Engine.Parsing
{
    public class FleetMovementPageParser : BaseParser
    {
        private static readonly Regex FleetIdRegex = new Regex(@"fleet([\d]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.Query.Contains("page=movement");
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            HtmlDocument doc = container.ResponseHtml.Value;
            HtmlNodeCollection imageFields = doc.DocumentNode.SelectNodes("//div[starts-with(@id, 'fleet')]");

            if (imageFields == null)
                yield break;

            FleetSlotCount count = new FleetSlotCount();
            HtmlNode slots = doc.DocumentNode.SelectSingleNode("//div[@id='content']//div[@class='fleetStatus']/span[@class='fleetSlots']");
            count.Current = int.Parse(slots.SelectSingleNode("./span[@class='current']").InnerText);
            count.Max = int.Parse(slots.SelectSingleNode("./span[@class='all']").InnerText);

            yield return count;

            foreach (HtmlNode node in imageFields)
            {
                string idText = node.GetAttributeValue("id", null);
                int id = int.Parse(FleetIdRegex.Match(idText).Groups[1].Value, NumberStyles.AllowThousands | NumberStyles.Integer, client.ServerCulture);

                FleetMissionDetails missionDetails = FleetUtilityParser.ParseFleetMissionDetails(node);
                HtmlNode fleetInfo = node.SelectSingleNode(".//span[@class='starStreak']");
                FleetComposition composition = FleetUtilityParser.ParseFleetInfoTable((OGameClient) client, fleetInfo);

                FleetEndpointInfo endpointOrigin = ParseEndpoint(node.SelectSingleNode("./span[@class='originData']"));
                FleetEndpointInfo endpointDestination = ParseEndpoint(node.SelectSingleNode("./span[@class='destinationData']"));

                yield return new FleetInfo
                {
                    Id = id,
                    ArrivalTime = missionDetails.ArrivalTime,
                    IsReturning = missionDetails.IsReturn,
                    MissionType = missionDetails.Mission,
                    Origin = endpointOrigin,
                    Destination = endpointDestination,
                    Composition = composition
                };
            }
        }

        private FleetEndpointInfo ParseEndpoint(HtmlNode node)
        {
            // destinationCoords
            HtmlNode coordsNode = node.SelectSingleNode(".//span[contains(@class, 'originCoords') or contains(@class, 'destinationCoords')]");
            HtmlNode planetNode = node.SelectSingleNode(".//span[contains(@class, 'originPlanet') or contains(@class, 'destinationPlanet')]");

            HtmlNode typeNode = node.SelectSingleNode(".//figure");
            string typeClass = typeNode?.GetAttributeValue("class", string.Empty) ?? string.Empty;

            CoordinateType type = CoordinateType.Unknown;

            if (typeClass.Contains(" planet ")) type = CoordinateType.Planet;
            else if (typeClass.Contains(" moon ")) type = CoordinateType.Moon;
            else if (typeClass.Contains(" tf ")) type = CoordinateType.DebrisField;

            string coordsText = coordsNode.InnerText;
            string playerName = coordsNode.GetAttributeValue("title", "");
            string planetName = planetNode.InnerText;

            return new FleetEndpointInfo
            {
                // TODO: planet always
                Coordinate = Coordinate.Parse(coordsText, type),
                EndpointName = planetName,
                Playername = playerName
            };
        }
    }
}