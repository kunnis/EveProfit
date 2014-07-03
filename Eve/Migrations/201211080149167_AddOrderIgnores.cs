namespace Eve.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddOrderIgnores : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrderToIgnores",
                c => new
                    {
                        OrderID = c.Long(nullable: false, identity: true),
                        IgnoreDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.OrderID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.OrderToIgnores");
        }
    }
}
