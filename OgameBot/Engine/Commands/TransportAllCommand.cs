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
        public Coordinate Destination { get; set; }
        public int Speed { get; set; } = 10;
        public bool UseDeployment { get; set; } = false;

        protected override CommandQueueElement RunInternal()
        {
            var resp = Client.IssueRequest(Client.RequestBuilder.GetPage(PageType.Fleet, PlanetId));
            PlanetResources resources = resp.GetParsedSingle<PlanetResources>();
            DetectedShip cargo = resp.GetParsed<DetectedShip>().FirstOrDefault(s => s.Ship == ShipType.LargeCargo);

            FleetComposition fleet = FleetComposition.ToTransport(resources.Resources);
            // if on a moon, leave 10% of deuterium, or else no other ship will be able to travel
            if (resp.GetParsedSingle<OgamePageInfo>().PlanetCoord.Type == CoordinateType.Moon)
            {
                fleet.Resources.Deuterium = (int)(fleet.Resources.Deuterium * 0.9f);
            }
            int needed = fleet.Ships[ShipType.LargeCargo];
            int available = cargo?.Count ?? 0;

            if (needed > available)
            {
                Logger.Instance.Log(LogLevel.Error, $"Not enough Large Cargos on planet: needed {needed}, only {available} available.");
                return null;
            }

            SendFleetCommand sendFleet = new SendFleetCommand()
            {
                Mission = UseDeployment ? MissionType.Deployment : MissionType.Transport,
                Speed = Speed,
                PlanetId = PlanetId,
                Destination = Destination,
                Fleet = fleet
            };
            sendFleet.Run();
            return null;
        }
    }
}
