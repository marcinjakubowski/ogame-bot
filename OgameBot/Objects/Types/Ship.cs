namespace OgameBot.Objects.Types
{
    public class Ship : BaseCostEntity<ShipType, Ship>
    {
        public static Ship LightFighter { get; } = new Ship(ShipType.LightFighter, new Resources(3000, 1000, 0));
        public static Ship HeavyFighter { get; } = new Ship(ShipType.HeavyFighter, new Resources(6000, 4000, 0));
        public static Ship Cruiser { get; } = new Ship(ShipType.Cruiser, new Resources(20000, 7000, 2000));
        public static Ship Battleship { get; } = new Ship(ShipType.Battleship, new Resources(45000, 15000, 0));
        public static Ship Battlecruiser { get; } = new Ship(ShipType.Battlecruiser, new Resources(30000, 40000, 15000));
        public static Ship Bomber { get; } = new Ship(ShipType.Bomber, new Resources(50000, 25000, 15000));
        public static Ship Destroyer { get; } = new Ship(ShipType.Destroyer, new Resources(60000, 50000, 15000));
        public static Ship Deathstar { get; } = new Ship(ShipType.Deathstar, new Resources(5000000, 4000000, 1000000));
        public static Ship SmallCargo { get; } = new Ship(ShipType.SmallCargo, new Resources(2000, 2000, 0));
        public static Ship LargeCargo { get; } = new Ship(ShipType.LargeCargo, new Resources(6000, 6000, 0));
        public static Ship Colony { get; } = new Ship(ShipType.ColonyShip, new Resources(10000, 20000, 10000));
        public static Ship Recycler { get; } = new Ship(ShipType.Recycler, new Resources(10000, 6000, 2000));
        public static Ship EspionageProbe { get; } = new Ship(ShipType.EspionageProbe, new Resources(0, 1000, 0));
        public static Ship SolarSatellite { get; } = new Ship(ShipType.SolarSatellite, new Resources(0, 2000, 500));


        static Ship()
        {
        }

        private Ship(ShipType type, Resources cost) : base(type, cost) { }


        public static implicit operator Ship(ShipType type)
        {
            return Index[type];
        }

        public static implicit operator ShipType(Ship type)
        {
            return type.Type;
        }
    }
}