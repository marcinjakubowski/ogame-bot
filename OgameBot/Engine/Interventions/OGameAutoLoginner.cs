using System;
using System.Net;
using System.Net.Http;
using ScraperClientLib.Engine;
using ScraperClientLib.Engine.Interventions;
using OgameBot.Logging;

namespace OgameBot.Engine.Interventions
{
    public class OGameAutoLoginner : IInterventionHandler
    {
        private readonly OGameClient _client;
        private int _loginRetry = 0;

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
            _loginRetry = 0;
            return false;
        }

        public InterventionResult Handle(ResponseContainer offendingTask)
        {
            if( ++_loginRetry > 1 )
            {
                Logger.Instance.Log(LogLevel.Error, "Login failed!");
                return new InterventionResult(InterventionResultState.Abort);
            }
            // Build login request
            Logger.Instance.Log(LogLevel.Warning, "Login necessary");
            HttpRequestMessage loginReq = _client.PrepareLogin();
            return new InterventionResult(InterventionResultState.RetryCurrentTask, loginReq, () => _client.SaveCookies());
        }
    }
}
