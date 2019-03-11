namespace DbAcceess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CanvasItemDetail",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CanvasWorkId = c.Long(nullable: false),
                        ShapeType = c.String(),
                        Index = c.Int(nullable: false),
                        Height = c.Double(nullable: false),
                        Width = c.Double(nullable: false),
                        MinHeight = c.Double(nullable: false),
                        MinWidth = c.Double(nullable: false),
                        Data = c.String(),
                        XOffSet = c.Double(nullable: false),
                        YOffSet = c.Double(nullable: false),
                        Stroke = c.String(),
                        FillString = c.String(),
                        Fill = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CanvasWork", t => t.CanvasWorkId, cascadeDelete: true)
                .Index(t => t.CanvasWorkId);
            
            CreateTable(
                "dbo.CanvasWork",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        DateStored = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CanvasItemDetail", "CanvasWorkId", "dbo.CanvasWork");
            DropIndex("dbo.CanvasItemDetail", new[] { "CanvasWorkId" });
            DropTable("dbo.CanvasWork");
            DropTable("dbo.CanvasItemDetail");
        }
    }
}
