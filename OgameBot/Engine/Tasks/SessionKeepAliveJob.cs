using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
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
        private Random _rng = new Random();

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
        }

        public override void Start()
        {
            if (Mode == SessionKeepAliveMode.Single && PlanetId == 0)
                throw new ArgumentException("PlanetId has to be specified when Single mode is selected", nameof(PlanetId));

            base.Start();
        }

        protected override void RunInternal()
        {
            var req = _client.RequestBuilder.GetEventList();
            var resp = _client.IssueRequest(req);
            var events = resp.GetParsed<EventInfo>();

            int own = 0, friendly = 0, hostile = 0;
            foreach (EventInfo ev in events)
            {
                switch (ev.Type)
                {
                    case EventType.Own:
                        own++; break;
                    case EventType.Friendly:
                        friendly++; break;
                    case EventType.Hostile:
                        hostile++; break;
                }
            }
            Logger.Instance.Log(LogLevel.Info, $"Events: {own} own, {friendly} friendly, {hostile} hostile");

            switch (Mode)
            {
                case SessionKeepAliveMode.All:
                    _planets.ForEach(x => PingPlanet(x));
                    break;
                case SessionKeepAliveMode.Random:
                    Random rand = new Random();
                    PingPlanet(_planets[rand.Next(_planets.Count)]);
                    break;
                case SessionKeepAliveMode.RoundRobin:
                    PingPlanet(_planets[_roundRobin++]);
                    if (_roundRobin == _planets.Count) _roundRobin = 0;
                    break;
                case SessionKeepAliveMode.Single:
                    PingPlanet(PlanetId);
                    break;
                case SessionKeepAliveMode.Last:
                    PingPlanet(null);
                    break;
                default:
                    throw new ArgumentException("Unknown mode", nameof(Mode));
            }

            ExecutionInterval = TimeSpan.FromMinutes(4) + TimeSpan.FromSeconds(_rng.Next(180));
        }

        private void PingPlanet(int? cp)
        {
            HttpRequestMessage req = _client.RequestBuilder.GetPage(Objects.Types.PageType.Overview, cp);
            _client.IssueRequest(req);
        }
    }
}