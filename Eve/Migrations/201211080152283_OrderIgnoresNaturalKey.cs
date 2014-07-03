namespace Eve.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderIgnoresNaturalKey : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderToIgnores", "OrderID", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OrderToIgnores", "OrderID", c => c.Long(nullable: false, identity: true));
        }
    }
}
