using System;
using System.Collections.Generic;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Parsing;
using HtmlAgilityPack;
using OgameBot.Engine.Parsing.UtilityParsers;
using OgameBot.Objects;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects.Types;
using System.Net;
using System.Text.RegularExpressions;

namespace OgameBot.Engine.Parsing
{
    public class EventListParser : BaseParser
    {
        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.Query.Contains("page=eventList");
        }

        private static char[] trims = { ' ', '\n', '\r', '\t' };

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            HtmlDocument doc = container.ResponseHtml.Value;
            HtmlNodeCollection imageFields = doc.DocumentNode.SelectNodes("//tr[starts-with(@id, 'eventRow')]");

            if (imageFields == null)
                yield break;

            foreach (HtmlNode node in imageFields)
            {
                FleetMissionDetails missionDetails = FleetUtilityParser.ParseFleetMissionDetails(node);
                EventInfo info = new EventInfo()
                {
                    ArrivalTime = missionDetails.ArrivalTime,
                    IsReturning = missionDetails.IsReturn,
                    MissionType = missionDetails.Mission
                };
                
                string idText = node.GetAttributeValue("id", null);
                info.Id = int.Parse(idText.Substring(9, idText.Length - 9));

                string @class = node.ChildNodes[1].GetAttributeValue("class", string.Empty);
                if (@class.Contains("friendly"))
                {
                    info.Type = EventType.Own;
                }
                else if (@class.Contains("hostile"))
                {
                    info.Type = EventType.Hostile;
                }
                else
                {
                    throw new ApplicationException($"Fleet is neither own nor hostile: {@class}");
                }
                
                // crashes on missle attack, need to get the html to parse correctly
                string detailsHtml = WebUtility.HtmlDecode(node.SelectSingleNode("./td[contains(@class, 'icon_movement')]")?.ChildNodes[1].GetAttributeValue("title", string.Empty));
                if (detailsHtml != null)
                {
                    HtmlDocument fleetComposition = new HtmlDocument();
                    fleetComposition.LoadHtml(detailsHtml);
                    info.Composition = FleetUtilityParser.ParseFleetInfoTable((OGameClient)client, fleetComposition.DocumentNode);
                }

                string playerOther = node.SelectSingleNode("./td[@class='sendMail']/a")?.GetAttributeValue("title", string.Empty) ?? string.Empty;

                info.Origin = new FleetEndpointInfo()
                {
                    Coordinate = Coordinate.Parse(node.SelectSingleNode("./td[@class='coordsOrigin']").InnerText.Trim(trims), CoordinateType.Planet),
                    EndpointName = node.SelectSingleNode("./td[@class='originFleet']").InnerText.Trim(trims),
                    Playername = info.Type == EventType.Own ? "" : playerOther
                };

                info.Destination = new FleetEndpointInfo()
                {
                    Coordinate = Coordinate.Parse(node.SelectSingleNode("./td[@class='destCoords']").InnerText.Trim(trims), CoordinateType.Planet),
                    EndpointName = node.SelectSingleNode("./td[@class='destFleet']").InnerText.Trim(trims),
                    Playername = info.Type == EventType.Own ? playerOther : ""
                };

                yield return info;
            }
        }

        private FleetEndpointInfo ParseEndpoint(HtmlNode node)
        {
            // destinationCoords
            HtmlNode coordsNode = node.SelectSingleNode(".//span[contains(@class, 'originCoords') or contains(@class, 'destinationCoords')]");
            HtmlNode planetNode = node.SelectSingleNode(".//span[contains(@class, 'originPlanet') or contains(@class, 'destinationPlanet')]");

            string coordsText = coordsNode.InnerText;
            string playerName = coordsNode.GetAttributeValue("title", "");
            string planetName = planetNode.InnerText;

            return new FleetEndpointInfo
            {
                // TODO: planet always
                Coordinate = Coordinate.Parse(coordsText, CoordinateType.Planet),
                EndpointName = planetName,
                Playername = playerName
            };
        }


    }
}
