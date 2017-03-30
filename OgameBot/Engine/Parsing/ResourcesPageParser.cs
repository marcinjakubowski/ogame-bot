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
    public class ResourcesPageParser : BaseParser
    {
        private static readonly Regex CssRegex = new Regex(@"supply[\d]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.Query.Contains("page=resources");
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            HtmlDocument doc = container.ResponseHtml.Value;
            HtmlNodeCollection imageFields = doc.DocumentNode.SelectNodes("//div[@id='buttonz']/div[@class='content']//a[contains(@class, 'detail_button')]");

            if (imageFields == null)
                yield break;

            foreach (HtmlNode node in imageFields)
            {
                string cssClass = node.GetAttributeValue("ref", "");

                Building type;
                switch (cssClass)
                {
                    case "1":
                        type = Building.MetalMine;
                        break;
                    case "2":
                        type = Building.CrystalMine;
                        break;
                    case "3":
                        type = Building.DeuteriumSynthesizer;
                        break;
                    case "4":
                        type = Building.SolarPlant;
                        break;
                    case "12":
                        type = Building.FusionReactor;
                        break;
                    case "22":
                        type = Building.MetalStorage;
                        break;
                    case "23":
                        type = Building.CrystalStorage;
                        break;
                    case "24":
                        type = Building.DeuteriumTank;
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