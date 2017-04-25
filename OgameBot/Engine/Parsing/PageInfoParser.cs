using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Parsing;
using System.Collections.Specialized;
using OgameBot.Objects.Types;
using System;
using System.Linq;
using System.Web;
using OgameBot.Logging;

namespace OgameBot.Engine.Parsing
{
    public class PageInfoParser : BaseParser
    {
        private static readonly Regex FleetTokenRegex = new Regex(@"var miniFleetToken[\s]*=[\s]*[""']([a-fA-F0-9]*?)[""']", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.IsHtmlResponse;
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            HtmlDocument doc = container.ResponseHtml.Value;
            HtmlNodeCollection metaFields = doc.DocumentNode.SelectNodes("/html/head/meta[@name and @content]");

            if (metaFields == null)
                yield break;

            OgamePageInfo res = new OgamePageInfo();

            // Meta info
            foreach (HtmlNode field in metaFields)
            {
                string key = field.GetAttributeValue("name", string.Empty);
                string value = field.GetAttributeValue("content", string.Empty);

                res.Fields[key] = value;
            }

            // Page info: make use of the implicit Page <-> PageType casts to find the matching link
            NameValueCollection query = HttpUtility.ParseQueryString(container.RequestMessage.RequestUri.Query);
            if (query["page"] != null)
            {
                IEnumerable<Page> pages = Page.All(); //will this work or cause an exception?
                try
                {
                    res.Page = pages.Where(s => s.Link == query["page"]).FirstOrDefault();
                }
                catch (KeyNotFoundException)
                {
                    Logger.Instance.Log(LogLevel.Error, $"Page {query["page"]} could not be identified, {nameof(PageType)} missing.");
                }
                
            }

            // JS Vars
            // <script type="text/javascript">
            HtmlNodeCollection scriptBlocks = doc.DocumentNode.SelectNodes("//script[@type='text/javascript' and not(@src)]");
            if (scriptBlocks != null)
            {
                foreach (HtmlNode block in scriptBlocks)
                {
                    Match match = FleetTokenRegex.Match(block.InnerText);
                    if (!match.Success)
                        continue;

                    res.MiniFleetToken = match.Groups[1].Value;

                    break;
                }
            }

            HtmlNode token = doc.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='token']");
            if (token != null)
            {
                res.OrderToken = token.GetAttributeValue("value", string.Empty);
            }

            yield return res;
        }
    }
}