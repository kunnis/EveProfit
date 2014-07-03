using System;
using System.Collections.Generic;

namespace Eve.Domain
{
    public class OrderSet
    {
        public int OrderSetId { get; set; }
        public int TypeId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int? RegionId { get; set; }

        public List<Order> Orders { get; set; }
    }
}