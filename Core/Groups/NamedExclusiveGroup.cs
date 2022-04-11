namespace Svelto.ECS
{
    /// <summary>
    /// still experimental alternative to ExclusiveGroup, use this like:
    /// use this like:
    /// public class TriggersGroup : ExclusiveGroup<TriggersGroup> {}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class NamedExclusiveGroup<T>
    {
        public static ExclusiveGroup Group = new ExclusiveGroup();
        public static string         name  = typeof(T).FullName;

        static NamedExclusiveGroup()
        {
#if DEBUG && !PROFILE_SVELTO
            GroupNamesMap.idToName[Group] = $"{name} ID {Group.id}";
#endif            
            //The hashname is independent from the actual group ID. this is fundamental because it is want
            //guarantees the hash to be the same across different machines
            GroupHashMap.RegisterGroup(Group, $"{name}");
        }
        //      protected NamedExclusiveGroup(string recognizeAs) : base(recognizeAs)  {}
        //    protected NamedExclusiveGroup(ushort range) : base(range) {}
    }
}