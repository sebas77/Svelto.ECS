namespace Svelto.ECS
{
    public struct FilterContextID
    {
        public readonly uint id;

        internal FilterContextID(uint id)
        {
            DBC.ECS.Check.Require(id < ushort.MaxValue, "too many types registered, HOW :)");

            this.id = id;
        }

        public static FilterContextID GetNewContextID()
        {
            return EntitiesDB.SveltoFilters.GetNewContextID();
        }
    }
}