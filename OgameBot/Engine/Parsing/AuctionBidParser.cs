using ScraperClientLib.Engine.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScraperClientLib.Engine;
using System.Net.Http;
using OgameBot.Engine.Parsing.Objects;
using System.Text.RegularExpressions;
using OgameBot.Objects.Types;
using System.Globalization;
using OgameBot.Logging;
using Newtonsoft.Json;

namespace OgameBot.Engine.Parsing
{
    public class AuctionBidParser : BaseParser
    {
        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.AbsoluteUri.Contains("page=auctioneer") && container.RequestMessage.Method == HttpMethod.Post;
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            AuctionBidResponse response = null;
            try
            {
                response = JsonConvert.DeserializeObject<AuctionBidResponse>(container.Raw.Value);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogException(ex);
            }

            yield return response;
        }
    }
}
