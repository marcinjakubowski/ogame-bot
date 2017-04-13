using System.Collections.Generic;
using System.Net.Http;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Parsing;
using Newtonsoft.Json;

namespace OgameBot.Engine.Commands
{
    public abstract class CommandBase
    {
        public int PlanetId { get; set; }

        [JsonIgnore]
        public List<DataObject> ParsedObjects { get; }

        protected OGameClient Client => OGameClient.Instance;

        protected CommandBase()
        {
            ParsedObjects = new List<DataObject>();
        }

        protected ResponseContainer AssistedIssue(HttpRequestMessage request)
        {
            ResponseContainer result = Client.IssueRequest(request);
            ParsedObjects.AddRange(result.ParsedObjects);

            return result;
        }

        public abstract void Run();
    }
}
