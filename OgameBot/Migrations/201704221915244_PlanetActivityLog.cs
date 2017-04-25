namespace OgameBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlanetActivityLog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PlanetActivityLogs",
                c => new
                    {
                        LocationId = c.Long(nullable: false),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        Activity = c.Int(),
                    })
                .PrimaryKey(t => new { t.LocationId, t.CreatedOn })
                .ForeignKey("dbo.Planets", t => t.LocationId, cascadeDelete: true)
                .Index(t => t.LocationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PlanetActivityLogs", "LocationId", "dbo.Planets");
            DropIndex("dbo.PlanetActivityLogs", new[] { "LocationId" });
            DropTable("dbo.PlanetActivityLogs");
        }
    }
}
