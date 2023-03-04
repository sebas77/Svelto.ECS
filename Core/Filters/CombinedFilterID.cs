namespace Svelto.ECS
{
    public readonly struct CombinedFilterID
    {
        internal readonly long            id;
        
        public          FilterContextID contextID => new FilterContextID((uint)((id & 0xFFFF0000) >> 16));
        public          uint            filterID   => (uint)(id >> 32);

        public CombinedFilterID(int filterID, FilterContextID contextID)
        {
            id = (long)filterID << 32 | (uint)contextID.id << 16;
        }

        public static implicit operator CombinedFilterID((int filterID, FilterContextID contextID) data)
        {
            return new CombinedFilterID(data.filterID, data.contextID);
        }
    }
}