using OgameBot.Db.Parts;
using System;
using System.Collections.Generic;

namespace OgameBot.Objects.Types
{
    public class Ship : BaseCostEntity<ShipType, Ship>
    {
        private int _baseSpeed;
        private ResearchType _speedResearch;
        private int _speedBonus;

        public static Ship LightFighter { get; } = new Ship(ShipType.LightFighter, new Resources(3000, 1000, 0), 12500, ResearchType.CombustionDrive);
        public static Ship HeavyFighter { get; } = new Ship(ShipType.HeavyFighter, new Resources(6000, 4000, 0), 10000, ResearchType.ImpulseDrive);
        public static Ship Cruiser { get; } = new Ship(ShipType.Cruiser, new Resources(20000, 7000, 2000), 15000, ResearchType.ImpulseDrive);
        public static Ship Battleship { get; } = new Ship(ShipType.Battleship, new Resources(45000, 15000, 0), 10000, ResearchType.HyperspaceDrive);
        public static Ship Battlecruiser { get; } = new Ship(ShipType.Battlecruiser, new Resources(30000, 40000, 15000), 10000, ResearchType.HyperspaceDrive);
        public static Ship Bomber { get; } = new Ship(ShipType.Bomber, new Resources(50000, 25000, 15000), 4000, ResearchType.ImpulseDrive);
        public static Ship Destroyer { get; } = new Ship(ShipType.Destroyer, new Resources(60000, 50000, 15000), 5000, ResearchType.HyperspaceDrive);
        public static Ship Deathstar { get; } = new Ship(ShipType.Deathstar, new Resources(5000000, 4000000, 1000000), 100, ResearchType.HyperspaceDrive);
        public static Ship SmallCargo { get; } = new Ship(ShipType.SmallCargo, new Resources(2000, 2000, 0), 10000, ResearchType.ImpulseDrive);
        public static Ship LargeCargo { get; } = new Ship(ShipType.LargeCargo, new Resources(6000, 6000, 0), 7500, ResearchType.CombustionDrive);
        public static Ship Colony { get; } = new Ship(ShipType.ColonyShip, new Resources(10000, 20000, 10000), 2500, ResearchType.ImpulseDrive);
        public static Ship Recycler { get; } = new Ship(ShipType.Recycler, new Resources(10000, 6000, 2000), 2000, ResearchType.CombustionDrive);
        public static Ship EspionageProbe { get; } = new Ship(ShipType.EspionageProbe, new Resources(0, 1000, 0), 100000000, ResearchType.CombustionDrive);
        public static Ship SolarSatellite { get; } = new Ship(ShipType.SolarSatellite, new Resources(0, 2000, 500));

        public int GetSpeed(int researchLevel)
        {
            return _baseSpeed + researchLevel * _speedBonus;
        }

        public int GetSpeed(PlayerResearch research)
        {
            return GetSpeed(((Dictionary<ResearchType, int>)research)[_speedResearch]);
        }

        static Ship()
        {
        }

        private Ship(ShipType type, Resources cost) : this(type, cost, 0, ResearchType.Unknown)
        {
        }

        private Ship(ShipType type, Resources cost, int speed, ResearchType engineResearchType) : base(type, cost)
        {
            _baseSpeed = speed;
            _speedResearch = engineResearchType;

            float factor = 0.0f;
            switch (engineResearchType)
            {
                case ResearchType.ImpulseDrive:
                    factor = 0.5f;
                    break;
                case ResearchType.HyperspaceDrive:
                    factor = 0.3f;
                    break;
                case ResearchType.CombustionDrive:
                    factor = 0.1f;
                    break;
                case ResearchType.Unknown:
                    factor = 0.0f;
                    break;
                default:
                    throw new ArgumentException("Must be Impulse, Combustion or Hyperspace", nameof(engineResearchType));
            }

            _speedBonus = (int)(speed * factor);
        }


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