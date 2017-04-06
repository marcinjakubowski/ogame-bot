using OgameBot.Db;
using OgameBot.Engine.Parsing.Objects;
using OgameBot.Logging;
using OgameBot.Objects;
using OgameBot.Objects.Types;
using System.Linq;

namespace OgameBot.Engine.Commands
{
    public class TransportAllCommand : CommandBase
    {
        public Coordinate Source { get; set; }
        public Coordinate Destination { get; set; }
        public int Speed { get; set; } = 10;
        public bool UseDeployment { get; set; } = false;


        private int _from;

        public TransportAllCommand(OGameClient client) : base(client)
        {
        }

        public TransportAllCommand(OGameClient client, int fromId, int toId) : base(client)
        {
            _from = fromId;

            using (BotDb db = new BotDb())
            {
                Destination = db.Planets.Where(p => p.PlanetId == toId).Select(p => p.LocationId).First();
                Source = db.Planets.Where(p => p.PlanetId == fromId).Select(p => p.LocationId).First();
            }
        }

        public override void Run()
        {
            if (_from == 0)
            {
                using (BotDb db = new BotDb())
                {
                    _from = (int)db.Planets.Where(s => s.LocationId == Source.Id).Select(s => s.PlanetId).First();
                }
            }

            var resp = Client.IssueRequest(Client.RequestBuilder.GetPage(PageType.Fleet, _from));
            PlanetResources resources = resp.GetParsedSingle<PlanetResources>();
            DetectedShip cargo = resp.GetParsed<DetectedShip>().Where(s => s.Ship == ShipType.LargeCargo).FirstOrDefault();

            FleetComposition fleet = FleetComposition.ToTransport(resources.Resources);
            int needed = fleet.Ships[ShipType.LargeCargo];
            int available = cargo != null ? cargo.Count : 0;

            if (cargo == null || needed > cargo.Count)
            {
                Logger.Instance.Log(LogLevel.Error, $"Not enough Large Cargos on planet: needed {needed}, only {available} available.");
                return;
            }

            SendFleetCommand sendFleet = new SendFleetCommand(Client)
            {
                Mission = UseDeployment ? MissionType.Deployment : MissionType.Transport,
                Speed = Speed,
                Source = Source,
                Destination = Destination,
                Fleet = fleet
            };
            sendFleet.Run();

        }
    }
}
