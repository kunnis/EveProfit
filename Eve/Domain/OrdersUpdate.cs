using System;
using System.Collections.Generic;

namespace Eve.Domain
{
    public class OrdersUpdate
    {
        public int OrdersUpdateId { get; set; }
        public string Version { get; set; }
        public List<UploadKey> UploadKeys { get; set; }
        public string GeneratorName { get; set; }
        public string GeneratorVersion { get; set; }
        public DateTime MessageTimeStamp { get; set; }

        public List<OrderSet> OrderSets { get; set; }
    }
}