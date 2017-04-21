using ScraperClientLib.Engine.Parsing;
using System.Collections.Generic;
using ScraperClientLib.Engine;
using OgameBot.Engine.Parsing.Objects;
using ScraperClientLib.Utilities;
using System.Web;
using System.Collections.Specialized;

namespace OgameBot.Engine.Parsing
{
    public class FleetCheckParser : BaseParser
    {
        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.Query.Contains("page=fleetcheck");
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            var post = container.OriginalRequest.Content.ReadAsStringAsync().Sync();
            NameValueCollection postParams = HttpUtility.ParseQueryString(post);

            var response = new FleetCheck(byte.Parse(postParams["galaxy"]), short.Parse(postParams["system"]), byte.Parse(postParams["planet"]), int.Parse(postParams["type"]));

            if (container.Raw.Value == "0")
                response.Status = FleetCheckStatus.OK;
            else if (container.Raw.Value == "1d")
                response.Status = FleetCheckStatus.NoDebrisField;

            yield return response;
        }

        
    }
}
