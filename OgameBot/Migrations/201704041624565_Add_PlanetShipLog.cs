namespace OgameBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_PlanetShipLog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PlanetShipLogs",
                c => new
                    {
                        LocationId = c.Long(nullable: false),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
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
                    })
                .PrimaryKey(t => new { t.LocationId, t.CreatedOn })
                .ForeignKey("dbo.Planets", t => t.LocationId, cascadeDelete: true)
                .Index(t => t.LocationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PlanetShipLogs", "LocationId", "dbo.Planets");
            DropIndex("dbo.PlanetShipLogs", new[] { "LocationId" });
            DropTable("dbo.PlanetShipLogs");
        }
    }
}
