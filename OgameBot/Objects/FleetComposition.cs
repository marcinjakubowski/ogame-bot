using System.Collections.Generic;
using System.Linq;
using OgameBot.Objects.Types;
using System;
using System.Text;
using OgameBot.Db.Parts;
using OgameBot.Engine.Parsing.Objects;

namespace OgameBot.Objects
{
    public class FleetComposition
    {
        public Dictionary<ShipType, int> Ships { get; private set; }

        public Resources Resources { get; set; }

        public int TotalShips => Ships.Sum(s => s.Value);

        public FleetComposition()
        {
            Ships = new Dictionary<ShipType, int>();
            Resources = new Resources();
        }

        public int Speed(PlayerResearch research)
        {
            return Ships.Keys.Min(s => ((Ship)s).GetSpeed(research));
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Ships.ToList().ForEach(s => sb.Append($"{s.Value}x {s.Key}, "));
            sb.Length -= 2;

            if (Resources.Total > 0)
            {
                sb.Append($" carrying {Resources}");
            }
            return sb.ToString();
        }

        public static FleetComposition ToTransport(Resources resources, ShipType cargo = ShipType.LargeCargo)
        {
            FleetComposition fleet = new FleetComposition();

            fleet.Ships[cargo] = (int)Math.Ceiling(resources.Total / GetTransportCapacity(cargo));
            fleet.Resources = resources;

            return fleet;
        }

        public static FleetComposition ToPlunder(Resources resources, ShipType cargo = ShipType.LargeCargo)
        {
            FleetComposition fleet = new FleetComposition();
            // Simple calculation method
            fleet.Ships[cargo] = (int)Math.Ceiling(GetPlunder(resources).Total / GetTransportCapacity(cargo));

            return fleet;
        }

        public static FleetComposition ToSpy(int probeCount)
        {
            FleetComposition fleet = new FleetComposition();
            fleet.Ships[ShipType.EspionageProbe] = probeCount;

            return fleet;
        }

        public static FleetComposition FromDetected(IEnumerable<DetectedShip> detected)
        {
            return new FleetComposition()
            {
                Ships = detected.Where(s => s.Count > 0).ToDictionary(s => s.Ship, s => s.Count)
            };
        }

        public static Resources GetPlunder(Resources available)
        {
            Resources theoretical = available / 2;
            theoretical.Energy = 0;

            return theoretical;
        }

        private static decimal GetTransportCapacity(ShipType cargo)
        {
            switch(cargo)
            {
                case ShipType.LargeCargo:
                    return 25000;
                case ShipType.SmallCargo:
                    return 10000;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cargo), "Can only transport using Large or Small Cargo");
            }
        }
    }
}