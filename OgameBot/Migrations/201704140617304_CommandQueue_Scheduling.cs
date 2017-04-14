namespace OgameBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CommandQueue_Scheduling : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CommandQueue", "ScheduledById", c => c.Int());
            CreateIndex("dbo.CommandQueue", "ScheduledById");
            CreateIndex("dbo.CommandQueue", "ScheduledAt");
            AddForeignKey("dbo.CommandQueue", "ScheduledById", "dbo.CommandQueue", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CommandQueue", "ScheduledById", "dbo.CommandQueue");
            DropIndex("dbo.CommandQueue", new[] { "ScheduledAt" });
            DropIndex("dbo.CommandQueue", new[] { "ScheduledById" });
            DropColumn("dbo.CommandQueue", "ScheduledById");
        }
    }
}
