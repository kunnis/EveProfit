namespace Eve.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrdersUpdates",
                c => new
                    {
                        OrdersUpdateId = c.Int(nullable: false, identity: true),
                        Version = c.String(),
                        GeneratorName = c.String(),
                        GeneratorVersion = c.String(),
                        MessageTimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OrdersUpdateId);
            
            CreateTable(
                "dbo.UploadKeys",
                c => new
                    {
                        UploadKeyId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Key = c.String(),
                        OrdersUpdate_OrdersUpdateId = c.Int(),
                    })
                .PrimaryKey(t => t.UploadKeyId)
                .ForeignKey("dbo.OrdersUpdates", t => t.OrdersUpdate_OrdersUpdateId)
                .Index(t => t.OrdersUpdate_OrdersUpdateId);
            
            CreateTable(
                "dbo.OrderSets",
                c => new
                    {
                        OrderSetId = c.Int(nullable: false, identity: true),
                        TypeId = c.Int(nullable: false),
                        GeneratedAt = c.DateTime(nullable: false),
                        RegionId = c.Int(),
                        OrdersUpdate_OrdersUpdateId = c.Int(),
                    })
                .PrimaryKey(t => t.OrderSetId)
                .ForeignKey("dbo.OrdersUpdates", t => t.OrdersUpdate_OrdersUpdateId)
                .Index(t => t.OrdersUpdate_OrdersUpdateId);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderID = c.Long(nullable: false, identity: true),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        VolRemaining = c.Int(nullable: false),
                        MinVolume = c.Int(nullable: false),
                        Bid = c.Boolean(nullable: false),
                        IssueDate = c.DateTime(nullable: false),
                        Duration = c.Int(nullable: false),
                        StationID = c.Int(nullable: false),
                        Range = c.Int(nullable: false),
                        StartingVolume = c.Int(nullable: false),
                        SolarSystemId = c.Int(),
                        OrderSet_OrderSetId = c.Int(),
                    })
                .PrimaryKey(t => t.OrderID)
                .ForeignKey("dbo.OrderSets", t => t.OrderSet_OrderSetId)
                .Index(t => t.OrderSet_OrderSetId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Orders", new[] { "OrderSet_OrderSetId" });
            DropIndex("dbo.OrderSets", new[] { "OrdersUpdate_OrdersUpdateId" });
            DropIndex("dbo.UploadKeys", new[] { "OrdersUpdate_OrdersUpdateId" });
            DropForeignKey("dbo.Orders", "OrderSet_OrderSetId", "dbo.OrderSets");
            DropForeignKey("dbo.OrderSets", "OrdersUpdate_OrdersUpdateId", "dbo.OrdersUpdates");
            DropForeignKey("dbo.UploadKeys", "OrdersUpdate_OrdersUpdateId", "dbo.OrdersUpdates");
            DropTable("dbo.Orders");
            DropTable("dbo.OrderSets");
            DropTable("dbo.UploadKeys");
            DropTable("dbo.OrdersUpdates");
        }
    }
}
