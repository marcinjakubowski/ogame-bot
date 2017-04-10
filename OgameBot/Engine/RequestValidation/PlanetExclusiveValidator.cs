using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OgameBot.Engine.RequestValidation
{
    public class PlanetExclusiveValidator : IRequestValidator
    {
        private OGameClient _client;

        public PlanetExclusiveValidator(OGameClient client)
        {
            _client = client;
        }
        public HttpRequestMessage Validate(HttpRequestMessage request)
        {
            string uri = request.RequestUri.ToString();
            string[] parts = uri.Split('?');
            if (parts.Length == 1) return request;

            var parameters = HttpUtility.ParseQueryString(parts[1]);


            if (parameters["cp"] == null)
                return request;

            int cp = int.Parse(parameters["cp"]);

            if ((_client.CurrentPlanetExclusiveOperation?.Operation.PlanetId ?? cp) == cp)
                return request;

            parameters["ogpe"] = "1";
            parameters.Remove("cp");

            request.RequestUri = new Uri(parts[0] + '?' + parameters.ToString());

            return request;
        }
    }
}
