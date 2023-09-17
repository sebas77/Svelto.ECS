using System.Runtime.CompilerServices;

namespace Svelto.ECS
{
    public readonly struct ExclusiveBuildGroup
    {
        internal ExclusiveBuildGroup(ExclusiveGroupStruct group, ushort range)
        {
            _range = range;
            this.group = group;
        }

        public static implicit operator ExclusiveBuildGroup(ExclusiveGroupStruct group)
        {
            return new ExclusiveBuildGroup(group, 0);
        }

        public static implicit operator ExclusiveBuildGroup(ExclusiveGroup group)
        {
            return new ExclusiveBuildGroup(group, 0);
        }

        public static implicit operator ExclusiveGroupStruct(ExclusiveBuildGroup group)
        {
            return group.group;
        }
        
        public static ExclusiveGroupStruct operator +(ExclusiveBuildGroup c1, uint c2)
        {
            DBC.ECS.Check.Require(c2 < c1._range, $"group out of range, {c2} max range is {c1._range}");
            
            return c1.group + c2;
        }
        
        public override string ToString()
        {
            return group.ToName();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal uint Offset(ExclusiveGroupStruct exclusiveGroupStruct)
        {
            DBC.ECS.Check.Require((exclusiveGroupStruct.id - group.id) < _range, "group out of range");
            var offset = (uint)exclusiveGroupStruct.id - (uint)group.id;
            return offset;
        }

        internal ExclusiveGroupStruct @group    { get; }

        readonly ushort _range;

        public   bool                 isInvalid => group == ExclusiveGroupStruct.Invalid;
    }
}