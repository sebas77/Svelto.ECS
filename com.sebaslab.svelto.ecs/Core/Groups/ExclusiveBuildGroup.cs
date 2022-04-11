namespace Svelto.ECS
{
    public readonly struct ExclusiveBuildGroup
    {
        internal ExclusiveBuildGroup(ExclusiveGroupStruct group)
        {
            this.group = group;
        }

        public static implicit operator ExclusiveBuildGroup(ExclusiveGroupStruct group)
        {
            return new ExclusiveBuildGroup(group);
        }

        public static implicit operator ExclusiveBuildGroup(ExclusiveGroup group)
        {
            return new ExclusiveBuildGroup(group);
        }

        public static implicit operator ExclusiveGroupStruct(ExclusiveBuildGroup group)
        {
            return group.group;
        }
        
        public override string ToString()
        {
            return this.group.ToName();
        }

        internal ExclusiveGroupStruct @group    { get; }
        public   bool                 isInvalid => group == ExclusiveGroupStruct.Invalid;
    }
}