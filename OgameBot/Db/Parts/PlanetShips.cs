using OgameBot.Objects.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OgameBot.Db.Parts
{
    [ComplexType]
    public class PlanetShips : Asset<ShipType>
    {
        public PlanetShips() : base()
        {
        }
        protected PlanetShips(Dictionary<ShipType, int> other) : base(other)
        {
        }
        public int? Battlecruiser
        {
            get { return TryGet(ShipType.Battlecruiser); }
            set { TrySet(ShipType.Battlecruiser, value); }
        }
        public int? Battleship
        {
            get { return TryGet(ShipType.Battleship); }
            set { TrySet(ShipType.Battleship, value); }
        }

        public int? Bomber
        {
            get { return TryGet(ShipType.Bomber); }
            set { TrySet(ShipType.Bomber, value); }
        }

        public int? ColonyShip
        {
            get { return TryGet(ShipType.ColonyShip); }
            set { TrySet(ShipType.ColonyShip, value); }
        }

        public int? Cruiser
        {
            get { return TryGet(ShipType.Cruiser); }
            set { TrySet(ShipType.Cruiser, value); }
        }

        public int? Deathstar
        {
            get { return TryGet(ShipType.Deathstar); }
            set { TrySet(ShipType.Deathstar, value); }
        }

        public int? Destroyer
        {
            get { return TryGet(ShipType.Destroyer); }
            set { TrySet(ShipType.Destroyer, value); }
        }

        public int? EspionageProbe
        {
            get { return TryGet(ShipType.EspionageProbe); }
            set { TrySet(ShipType.EspionageProbe, value); }
        }

        public int? HeavyFighter
        {
            get { return TryGet(ShipType.HeavyFighter); }
            set { TrySet(ShipType.HeavyFighter, value); }
        }

        public int? LargeCargo
        {
            get { return TryGet(ShipType.LargeCargo); }
            set { TrySet(ShipType.LargeCargo, value); }
        }

        public int? LightFighter
        {
            get { return TryGet(ShipType.LightFighter); }
            set { TrySet(ShipType.LightFighter, value); }
        }

        public int? Recycler
        {
            get { return TryGet(ShipType.Recycler); }
            set { TrySet(ShipType.Recycler, value); }
        }

        public int? SmallCargo
        {
            get { return TryGet(ShipType.SmallCargo); }
            set { TrySet(ShipType.SmallCargo, value); }
        }

        public int? SolarSatellite
        {
            get { return TryGet(ShipType.SolarSatellite); }
            set { TrySet(ShipType.SolarSatellite, value); }
        }

        public int TotalValue => _dict.Sum(s => ((Ship)s.Key).Cost.Total * s.Value);

        public static implicit operator PlanetShips(Dictionary<ShipType, int> type)
        {
            return new PlanetShips(type);
        }

        public static implicit operator Dictionary<ShipType, int>(PlanetShips type)
        {
            return type._dict;
        }
    }
}
