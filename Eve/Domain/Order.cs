using System;

namespace Eve.Domain
{
    public class Order
    {
        public decimal Price { get; set; }
        public int VolRemaining { get; set; }
        //public int Range { get; set; }
        public long OrderID { get; set; }
        //public int VolEntered { get; set; }
        public int MinVolume { get; set; }
        public bool Bid { get; set; }
        public DateTime IssueDate { get; set; }
        public int Duration { get; set; }
        public int StationID { get; set; }

        public int Range { get; set; }

        public int StartingVolume { get; set; }

        public int? SolarSystemId { get; set; }
    }
}