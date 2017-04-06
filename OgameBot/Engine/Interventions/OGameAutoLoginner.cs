using System;
using System.Net;
using System.Net.Http;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Interventions;

namespace OgameBot.Engine.Interventions
{
    public class OGameAutoLoginner : IInterventionHandler
    {
        private readonly OGameClient _client;

        public OGameAutoLoginner(OGameClient client)
        {
            _client = client;
        }

        public bool DoIntervention(ResponseContainer offendingTask)
        {
            Uri requestUri = offendingTask.RequestMessage.RequestUri;
            if (requestUri.DnsSafeHost != _client.BaseUri.DnsSafeHost)
            {
                return true;
            }

            return false;
        }

        public InterventionResult Handle(ResponseContainer offendingTask)
        {
            // Build login request
            HttpRequestMessage loginReq = _client.PrepareLogin();

            return new InterventionResult(InterventionResultState.RetryCurrentTask, loginReq);
        }
    }
}
