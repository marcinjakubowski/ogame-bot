using System.Collections.Generic;
using System.Linq;
using OgameBot.Objects.Types;
using System;

namespace OgameBot.Objects
{
    public class FleetComposition
    {
        public Dictionary<ShipType, int> Ships { get; }

        public Resources Resources { get; set; }

        public int TotalShips => Ships.Sum(s => s.Value);

        public FleetComposition()
        {
            Ships = new Dictionary<ShipType, int>();
            Resources = new Resources();
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