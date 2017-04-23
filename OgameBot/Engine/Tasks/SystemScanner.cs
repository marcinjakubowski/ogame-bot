using OgameBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OgameBot.Engine.Tasks
{
    public class SystemScanner : WorkerBase
    {
        private readonly List<SystemCoordinate> _systems;
        private OGameClient _client => OGameClient.Instance;
        public SystemScanner(IEnumerable<SystemCoordinate> systems)
        {
            ExecutionInterval = TimeSpan.FromMinutes(1);
            _systems = systems.ToList();
        }

        protected override void RunInternal()
        {
            _systems.ForEach(Scan);
        }

        private void Scan(SystemCoordinate coord)
        {
            _client.IssueRequest(_client.RequestBuilder.GetGalaxyContent(coord));
        }
    }
}
