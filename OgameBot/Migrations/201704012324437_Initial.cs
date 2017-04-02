namespace OgameBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DebrisFields",
                c => new
                    {
                        LocationId = c.Long(nullable: false),
                        Resources_Metal = c.Int(nullable: false),
                        Resources_Crystal = c.Int(nullable: false),
                        Resources_Deuterium = c.Int(nullable: false),
                        Resources_Energy = c.Int(nullable: false),
                        LastSeen = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.LocationId);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        MessageId = c.Int(nullable: false),
                        TabType = c.Int(nullable: false),
                        SerializedMessage = c.Binary(nullable: false, maxLength: 4096),
                        SerializedMessageType = c.String(nullable: false, maxLength: 128),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.MessageId);
            
            CreateTable(
                "dbo.Planets",
                c => new
                    {
                        LocationId = c.Long(nullable: false),
                        PlayerId = c.Int(),
                        Name = c.String(maxLength: 255),
                        Resources_Metal = c.Int(nullable: false),
                        Resources_Crystal = c.Int(nullable: false),
                        Resources_Deuterium = c.Int(nullable: false),
                        Resources_Energy = c.Int(nullable: false),
                        Buildings_AllianceDepot = c.Int(),
                        Buildings_CrystalMine = c.Int(),
                        Buildings_CrystalStorage = c.Int(),
                        Buildings_DeuteriumSynthesizer = c.Int(),
                        Buildings_DeuteriumTank = c.Int(),
                        Buildings_FusionReactor = c.Int(),
                        Buildings_JumpGate = c.Int(),
                        Buildings_LunarBase = c.Int(),
                        Buildings_MetalMine = c.Int(),
                        Buildings_MetalStorage = c.Int(),
                        Buildings_MissileSilo = c.Int(),
                        Buildings_NaniteFactory = c.Int(),
                        Buildings_ResearchLab = c.Int(),
                        Buildings_RoboticFactory = c.Int(),
                        Buildings_SensorPhalanx = c.Int(),
                        Buildings_Shipyard = c.Int(),
                        Buildings_SolarPlant = c.Int(),
                        Buildings_Terraformer = c.Int(),
                        Buildings_LastUpdated = c.DateTimeOffset(precision: 7),
                        Ships_Battlecruiser = c.Int(),
                        Ships_Battleship = c.Int(),
                        Ships_Bomber = c.Int(),
                        Ships_ColonyShip = c.Int(),
                        Ships_Cruiser = c.Int(),
                        Ships_Deathstar = c.Int(),
                        Ships_Destroyer = c.Int(),
                        Ships_EspionageProbe = c.Int(),
                        Ships_HeavyFighter = c.Int(),
                        Ships_LargeCargo = c.Int(),
                        Ships_LightFighter = c.Int(),
                        Ships_Recycler = c.Int(),
                        Ships_SmallCargo = c.Int(),
                        Ships_SolarSatellite = c.Int(),
                        Ships_LastUpdated = c.DateTimeOffset(precision: 7),
                        Defences_AntiBallisticMissiles = c.Int(),
                        Defences_GaussCannon = c.Int(),
                        Defences_HeavyLaser = c.Int(),
                        Defences_InterplanetaryMissiles = c.Int(),
                        Defences_IonCannon = c.Int(),
                        Defences_LargeShieldDome = c.Int(),
                        Defences_LightLaser = c.Int(),
                        Defences_PlasmaTurret = c.Int(),
                        Defences_RocketLauncher = c.Int(),
                        Defences_SmallShieldDome = c.Int(),
                        Defences_LastUpdated = c.DateTimeOffset(precision: 7),
                        PlanetId = c.Int(),
                        LastResourcesTime = c.DateTimeOffset(precision: 7),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.LocationId)
                .ForeignKey("dbo.Players", t => t.PlayerId)
                .Index(t => t.PlayerId);
            
            CreateTable(
                "dbo.Players",
                c => new
                    {
                        PlayerId = c.Int(nullable: false),
                        Name = c.String(maxLength: 128),
                        Ranking = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Research_ArmourTechnology = c.Int(),
                        Research_Astrophysics = c.Int(),
                        Research_CombustionDrive = c.Int(),
                        Research_ComputerTechnology = c.Int(),
                        Research_EnergyTechnology = c.Int(),
                        Research_EspionageTechnology = c.Int(),
                        Research_GravitonTechnology = c.Int(),
                        Research_HyperspaceDrive = c.Int(),
                        Research_HyperspaceTechnology = c.Int(),
                        Research_ImpulseDrive = c.Int(),
                        Research_IntergalacticResearchNetwork = c.Int(),
                        Research_IonTechnology = c.Int(),
                        Research_LaserTechnology = c.Int(),
                        Research_PlasmaTechnology = c.Int(),
                        Research_ShieldingTechnology = c.Int(),
                        Research_WeaponsTechnology = c.Int(),
                        Research_LastUpdated = c.DateTimeOffset(precision: 7),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.PlayerId);
            
            CreateTable(
                "dbo.GalaxyScans",
                c => new
                    {
                        LocationId = c.Int(nullable: false),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        LastScan = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.LocationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Planets", "PlayerId", "dbo.Players");
            DropIndex("dbo.Planets", new[] { "PlayerId" });
            DropTable("dbo.GalaxyScans");
            DropTable("dbo.Players");
            DropTable("dbo.Planets");
            DropTable("dbo.Messages");
            DropTable("dbo.DebrisFields");
        }
    }
}
