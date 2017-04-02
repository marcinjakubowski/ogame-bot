namespace OgameBot.Objects.Types
{
    public class Defence : BaseCostEntity<DefenceType, Defence>
    {
        public static Defence RocketLauncher { get; } = new Defence(DefenceType.RocketLauncher, new Resources(2000, 0, 0));
        public static Defence LightLaser { get; } = new Defence(DefenceType.LightLaser, new Resources(1500, 500, 0));
        public static Defence HeavyLaser { get; } = new Defence(DefenceType.HeavyLaser, new Resources(6000, 2000, 0));
        public static Defence GaussCannon { get; } = new Defence(DefenceType.GaussCannon, new Resources(20000, 15000, 2000));
        public static Defence IonCannon { get; } = new Defence(DefenceType.IonCannon, new Resources(2000, 6000, 0));
        public static Defence PlasmaTurret { get; } = new Defence(DefenceType.PlasmaTurret, new Resources(50000, 50000, 30000));
        public static Defence SmallShieldDome { get; } = new Defence(DefenceType.SmallShieldDome, new Resources(10000, 10000, 0));
        public static Defence LargeShieldDome { get; } = new Defence(DefenceType.LargeShieldDome, new Resources(50000, 50000, 0));
        public static Defence AntiBallisticMissiles { get; } = new Defence(DefenceType.AntiBallisticMissiles, new Resources()); // no structural integrity
        public static Defence InterplanetaryMissiles { get; } = new Defence(DefenceType.InterplanetaryMissiles, new Resources()); // no structural integrity

        private Defence(DefenceType type, Resources cost) : base(type, cost) { }

        public static implicit operator Defence(DefenceType type)
        {
            return Index[type];
        }

        public static implicit operator DefenceType(Defence type)
        {
            return type.Type;
        }
    }
}