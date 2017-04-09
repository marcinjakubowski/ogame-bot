namespace OgameBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BuildOrder : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BuildOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LocationId = c.Long(nullable: false),
                        Building = c.Int(nullable: false),
                        Level = c.Int(nullable: false),
                        CompletedAt = c.DateTimeOffset(precision: 7),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Planets", t => t.LocationId, cascadeDelete: true)
                .Index(t => t.LocationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BuildOrders", "LocationId", "dbo.Planets");
            DropIndex("dbo.BuildOrders", new[] { "LocationId" });
            DropTable("dbo.BuildOrders");
        }
    }
}
