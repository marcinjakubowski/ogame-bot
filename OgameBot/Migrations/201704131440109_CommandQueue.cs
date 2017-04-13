namespace OgameBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CommandQueue : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CommandQueue",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Parameters = c.String(nullable: false),
                        CommandType = c.String(nullable: false, maxLength: 128),
                        ScheduledAt = c.DateTimeOffset(precision: 7),
                        CreatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                        UpdatedOn = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CommandQueue");
        }
    }
}
