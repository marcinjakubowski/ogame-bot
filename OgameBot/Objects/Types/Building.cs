namespace OgameBot.Objects.Types
{
    public class Building : BaseGrowingCostEntity<BuildingType, Building>
    {
        public static Building AllianceDepot        { get; } = new Building(BuildingType.AllianceDepot       , new Cost(20000, 40000, 0, 2.0f));
        public static Building CrystalMine          { get; } = new Building(BuildingType.CrystalMine         , new Cost(48, 24, 0, 1.6f));
        public static Building CrystalStorage       { get; } = new Building(BuildingType.CrystalStorage      , new Cost(1000, 500, 0, 2.0f));
        public static Building DeuteriumSynthesizer { get; } = new Building(BuildingType.DeuteriumSynthesizer, new Cost(225, 75, 0, 1.5f));
        public static Building DeuteriumTank        { get; } = new Building(BuildingType.DeuteriumTank       , new Cost(1000, 1000, 0, 2.0f));
        public static Building FusionReactor        { get; } = new Building(BuildingType.FusionReactor       , new Cost(90, 360, 180, 1.8f));
        public static Building JumpGate             { get; } = new Building(BuildingType.JumpGate            , new Cost(0, 0, 0, 0.0f)); // #todo
        public static Building LunarBase            { get; } = new Building(BuildingType.LunarBase           , new Cost(0, 0, 0, 0.0f)); // #todo
        public static Building MetalMine            { get; } = new Building(BuildingType.MetalMine           , new Cost(60, 15, 0, 1.5f));
        public static Building MetalStorage         { get; } = new Building(BuildingType.MetalStorage        , new Cost(1000, 0, 0, 2.0f));
        public static Building MissileSilo          { get; } = new Building(BuildingType.MissileSilo         , new Cost(20000, 20000, 1000, 2.0f));
        public static Building NaniteFactory        { get; } = new Building(BuildingType.NaniteFactory       , new Cost(1000000, 500000, 100000, 2.0f));
        public static Building ResearchLab          { get; } = new Building(BuildingType.ResearchLab         , new Cost(200, 400, 200, 2.0f));
        public static Building RoboticFactory       { get; } = new Building(BuildingType.RoboticFactory      , new Cost(400, 120, 200, 2.0f));
        public static Building SensorPhalanx        { get; } = new Building(BuildingType.SensorPhalanx       , new Cost(0, 0, 0, 0.0f)); // #todo
        //public static Building SpaceDock          { get; } = new Building(BuildingType.SpaceDock           , new Cost(200, 50, 0, 2.0f));
        public static Building Shipyard             { get; } = new Building(BuildingType.Shipyard            , new Cost(400, 200, 100, 2.0f));
        public static Building SolarPlant           { get; } = new Building(BuildingType.SolarPlant          , new Cost(75, 30, 0, 1.5f));
        public static Building Terraformer          { get; } = new Building(BuildingType.Terraformer         , new Cost(0, 50000, 100000, 2.0f));

        static Building()
        {
        }


        private Building(BuildingType type, Cost cost) : base(type, cost) { }

        public static implicit operator Building(BuildingType type)
        {
            return Index[type];
        }

        public static implicit operator BuildingType(Building type)
        {
            return type.Type;
        }
    }
}