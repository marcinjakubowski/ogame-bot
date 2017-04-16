using OgameBot.Db;
using OgameBot.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace OgameBot.Engine.Tasks
{
    public class SessionKeepAliveJob : WorkerBase
    {
        private readonly TimeSpan _sessionAgeLimit = TimeSpan.FromMinutes(4);
        private readonly OGameClient _client;
        private readonly List<int> _planets;
        private int _roundRobin = 0;

        public SessionKeepAliveMode Mode { get; set; }
        public int PlanetId { get; set; } = 0;

        public SessionKeepAliveJob(OGameClient client, SessionKeepAliveMode mode) : this(client, mode, 0)
        {
        }

        public SessionKeepAliveJob(OGameClient client, SessionKeepAliveMode mode, int planetId)
        {
            _client = client;
            ExecutionInterval = TimeSpan.FromMinutes(5);

            using (BotDb db = new BotDb())
            {
                _planets = db.Planets.Where(p => p.PlanetId != null).Select(p => p.PlanetId.Value).ToList();
            }

            Mode = mode;
            PlanetId = planetId;
            if (Mode == SessionKeepAliveMode.Single && PlanetId == 0)
                throw new ArgumentException("PlanetId has to be specified when Single mode is selected", nameof(PlanetId));
        }

        protected override void RunInternal()
        {
            TimeSpan lastReqAge = DateTime.UtcNow - _client.LastRequestUtc;
            if (lastReqAge < _sessionAgeLimit)
                return;

            if (Mode == SessionKeepAliveMode.All)
            {
                _planets.ForEach(PingPlanet);
            }
            else if (Mode == SessionKeepAliveMode.Random)
            {
                Random rand = new Random();
                PingPlanet(_planets[rand.Next(_planets.Count)]);
            }
            else if (Mode == SessionKeepAliveMode.RoundRobin)
            {
                PingPlanet(_planets[_roundRobin++]);
                if (_roundRobin == _planets.Count) _roundRobin = 0;
            }
            else if (Mode == SessionKeepAliveMode.Single)
            {
                PingPlanet(PlanetId);
            }

            // Also get event list to make sure
            var req = _client.RequestBuilder.GetEventList();
            _client.IssueRequest(req);
        }

        private void PingPlanet(int cp)
        {
            HttpRequestMessage req = _client.RequestBuilder.GetPage(Objects.Types.PageType.Overview, cp);
            _client.IssueRequest(req);
        }
    }
}