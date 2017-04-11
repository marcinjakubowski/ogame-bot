using System;
using System.Collections.Generic;
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

    public class FacilitiesPageParser : BaseParser
    {
        private static readonly Regex CssRegex = new Regex(@"station[\d]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.Query.Contains("page=station");
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            HtmlDocument doc = container.ResponseHtml.Value;
            HtmlNodeCollection imageFields = doc.DocumentNode.SelectNodes("//div[@id='buttonz']/div[@class='content']//div[contains(@class, 'station')]");

            if (imageFields == null)
                yield break;

            foreach (HtmlNode node in imageFields)
            {
                string cssClss = node.GetCssClasses(CssRegex).FirstOrDefault();

                Building type;
                switch (cssClss)
                {
                    case "station14":
                        type = Building.RoboticFactory;
                        break;
                    case "station21":
                        type = Building.Shipyard;
                        break;
                    case "station31":
                        type = Building.ResearchLab;
                        break;
                    case "station34":
                        type = Building.AllianceDepot;
                        break;
                    case "station44":
                        type = Building.MissileSilo;
                        break;
                    case "station15":
                        type = Building.NaniteFactory;
                        break;
                    case "station33":
                        type = Building.Terraformer;
                        break;
                    case "station41":
                        type = Building.LunarBase;
                        break;
                    case "station42":
                        type = Building.SensorPhalanx;
                        break;
                    case "station43":
                        type = Building.JumpGate;
                        break;
                    case "station36":
                        type = Building.SpaceDock;
                        break;
                    default:
                        continue;
                }

                int level = node.SelectSingleNode(".//span[@class='level']").GetFirstNumberChildNode(client.ServerCulture);

                HtmlNode fastBuildLinkNode = node.SelectSingleNode(".//a[contains(@class, 'fastBuild')]");
                string fastBuildLink = fastBuildLinkNode?.GetAttributeValue("onclick", null).Split('\'')[1];

                yield return new DetectedBuilding
                {
                    Building = type,
                    Level = level,
                    UpgradeUri = fastBuildLink == null ? null : new Uri(fastBuildLink)
                };
            }
        }
    }
}