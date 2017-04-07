using System;
using System.Collections.Generic;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Parsing;
using OgameBot.Engine.Parsing.Objects;
using Newtonsoft.Json;

namespace OgameBot.Engine.Parsing
{
    public class MinifleetParser : BaseParser
    {
        public override bool ShouldProcessInternal(ResponseContainer container)
        {
            return container.RequestMessage.RequestUri.AbsoluteUri.Contains("page=minifleet");
        }

        public override IEnumerable<DataObject> ProcessInternal(ClientBase client, ResponseContainer container)
        {
            MinifleetResponse status = JsonConvert.DeserializeObject<MinifleetResponse>(container.Raw.Value);

            yield return status;
        }

        
    }
}
