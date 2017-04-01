namespace OgameBot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PlanetCoords_Computed : DbMigration
    {
        public override void Up()
        {
            Sql(@"ALTER TABLE dbo.Planets ADD Coords AS 
                  (
                      CONCAT((LocationId / POWER(CAST(2 AS BIGINT), 32)) & 0xFF, ':'
                            ,(LocationId / POWER(CAST(2 AS BIGINT), 16)) & 0xFFFF, ':'
                            ,(LocationId / POWER(CAST(2 AS BIGINT), 8)) & 0xFF
                            ,CASE(LocationId & 0xFF)
                                   WHEN 1 THEN ''
                                   WHEN 2 THEN ':M'
                                   WHEN 3 THEN ':DF'
                                   ELSE ':?'
                                 END)
                  )
            ");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Planets", "Coords");
        }
    }
}
