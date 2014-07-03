namespace Eve
{
    public class RegionAndTypeId
    {
        public RegionAndTypeId(int regionId, int typeId)
        {
            RegionId = regionId;
            TypeId = typeId;
        }

        public int RegionId { get; protected set; }
        public int TypeId { get; protected set; }

        public bool Equals(RegionAndTypeId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.RegionId == RegionId && other.TypeId == TypeId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (RegionAndTypeId)) return false;
            return Equals((RegionAndTypeId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (RegionId*397) ^ TypeId;
            }
        }
    }
}