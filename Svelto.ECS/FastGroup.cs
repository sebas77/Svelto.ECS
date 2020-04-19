#if later
namespace Svelto.ECS
{
    public class FastGroup
    {
        internal static uint entitiesCount;
        
        public FastGroup()
        {
            _group = ExclusiveGroupStruct.Generate(1);
         }

        public static implicit operator ExclusiveGroupStruct(FastGroup group)
        {
            return group._group;
        }

        public static explicit operator uint(FastGroup group)
        {
            return group._group;
        }

        readonly ExclusiveGroupStruct _group;
        public uint value => _group;
    }
}
#endif