using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eve.Domain;

namespace Eve.Database
{
    public class EveHistoryContext : DbContext
    {
        public EveHistoryContext()
        {
        }

        public DbSet<OrdersUpdate> OrdersUpdates { get; set; }
        public DbSet<OrderToIgnore> OrdersToIgnore { get; set; }
    }
}
