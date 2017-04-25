using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using OgameBot.Utilities;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Parsing;

namespace OgameBot.Engine.Parsing
{
    public class EspionageDetailsParser : BaseParser
    {
        private static readonly Regex ItemIdRegex = new Regex(@"(?:building|research|defense|tech)([\d]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.Query.Contains("page=messages") &&
                   container.RequestMessage.RequestUri.Query.Contains("tabid=" + (int)MessageTabType.FleetsEspionage) &&
                   container.RequestMessage.RequestUri.Query.Contains("messageId=");
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            HtmlDocument doc = container.ResponseHtml.Value;
            HtmlNode message = doc.DocumentNode.SelectSingleNode("//div[@class='detail_msg']");

            if (message == null)
                yield break;

            OGameClient oClient = (OGameClient)client;

            // Message info
            int messageId = message.GetAttributeValue("data-msg-id", 0);
            MessageTabType tabType = MessageTabType.FleetsEspionage;

            string dateText = message.SelectSingleNode(".//span[contains(@class, 'msg_date')]").InnerText;
            DateTime date = DateTime.ParseExact(dateText, "dd.MM.yyyy HH:mm:ss", oClient.ServerCulture, DateTimeStyles.AssumeUniversal).ToUniversalTime();

            EspionageReport result = new EspionageReport
            {
                MessageId = messageId,
                TabType = tabType,
                Sent = new DateTimeOffset(date).Add(-oClient.Settings.ServerUtcOffset).ToOffset(oClient.Settings.ServerUtcOffset)
            };

            // Establish location
            HtmlNode locationLink = message.SelectSingleNode(".//a[contains(@href, 'page=galaxy')]");
            // This might happen for a destroyed planet
            if (locationLink == null)
            {
                yield break;
            }
            string locationType = locationLink.SelectSingleNode("./figure").GetCssClasses(s => s == "moon" || s == "planet").First();

            CoordinateType coordinateType = locationType == "moon" ? CoordinateType.Moon : CoordinateType.Planet;
            result.Coordinate = Coordinate.Parse(locationLink.InnerText, coordinateType);

            // Activity
            string activityText = message.SelectSingleNode(".//div[@class='detail_txt']/div/font[@color='red']")?.InnerText;

            if (activityText != null)
            {
                result.Activity = int.Parse(activityText);
            }

            // Parts
            HtmlNodeCollection partsNodesList = message.SelectNodes(".//ul[@data-type and not(./li[@class='detail_list_fail'])]");
            Dictionary<string, HtmlNode> partsNodes = partsNodesList.ToDictionary(s => s.GetAttributeValue("data-type", ""));

            // Parts - Resources
            HtmlNode details;
            if (partsNodes.TryGetValue("resources", out details))
            {
                HtmlNodeCollection values = details.SelectNodes(".//span[@class='res_value']");
                Debug.Assert(values.Count == 4);

                var oneThousandAdd = oClient.ServerCulture.NumberFormat.NumberGroupSeparator + "000";

                string[] vals = values.Select(s => s.InnerText
                    .Replace("Mn", oneThousandAdd)
                    .Replace("Bn", oneThousandAdd + oneThousandAdd)).ToArray();

                Resources resources = new Resources
                {
                    Metal = int.Parse(vals[0], NumberStyles.AllowThousands | NumberStyles.Integer, oClient.ServerCulture),
                    Crystal = int.Parse(vals[1], NumberStyles.AllowThousands | NumberStyles.Integer, oClient.ServerCulture),
                    Deuterium = int.Parse(vals[2], NumberStyles.AllowThousands | NumberStyles.Integer, oClient.ServerCulture),
                    Energy = int.Parse(vals[3], NumberStyles.AllowThousands | NumberStyles.Integer, oClient.ServerCulture)
                };

                result.Resources = resources;
                result.Details |= ReportDetails.Resources;
            }

            // Parts - Ships
            if (partsNodes.TryGetValue("ships", out details))
            {
                result.DetectedShips = ParseList<ShipType>(oClient, details);
                result.Details |= ReportDetails.Ships;
            }

            // Parts - Defense
            if (partsNodes.TryGetValue("defense", out details))
            {
                result.DetectedDefence = ParseList<DefenceType>(oClient, details);
                result.Details |= ReportDetails.Defense;
            }

            // Parts - Buildings
            if (partsNodes.TryGetValue("buildings", out details))
            {
                result.DetectedBuildings = ParseList<BuildingType>(oClient, details);
                result.Details |= ReportDetails.Buildings;
            }

            // Parts - Research
            if (partsNodes.TryGetValue("research", out details))
            {
                result.DetectedResearch = ParseList<ResearchType>(oClient, details);
                result.Details |= ReportDetails.Research;
            }

            // Return 
            yield return result;
        }

        private Dictionary<T, int> ParseList<T>(ClientBase client, HtmlNode node) where T : struct
        {
            HtmlNodeCollection items = node.SelectNodes(".//li[@class='detail_list_el']");

            if (items == null)
                return null;

            Dictionary<T, int> res = new Dictionary<T, int>();

            foreach (HtmlNode item in items)
            {
                string @class = item.SelectSingleNode(".//img").GetCssClasses(ItemIdRegex).First();
                object id = int.Parse(ItemIdRegex.Match(@class).Groups[1].Value);

                string value = item.SelectSingleNode(".//span[@class='fright']").InnerText;
                int level = int.Parse(value, NumberStyles.AllowThousands | NumberStyles.Integer, client.ServerCulture);

                res.Add((T)id, level);
            }

            return res;
        }
    }
}