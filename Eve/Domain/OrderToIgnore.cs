using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eve.Domain
{
    public class OrderToIgnore
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [System.ComponentModel.DataAnnotations.Key]
        public long OrderID { get; set; }
        public DateTime IgnoreDate { get; set; }
    }
}