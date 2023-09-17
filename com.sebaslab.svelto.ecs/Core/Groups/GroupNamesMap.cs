using System.Collections.Generic;
using Svelto.ECS;

static class GroupNamesMap
{
#if DEBUG
    static GroupNamesMap() { idToName = new Dictionary<ExclusiveGroupStruct, string>(); }

    internal static readonly Dictionary<ExclusiveGroupStruct, string> idToName;

    public static string ToName(this in ExclusiveGroupStruct group)
    {
        Dictionary<ExclusiveGroupStruct, string> idToName = GroupNamesMap.idToName;
        if (idToName.TryGetValue(@group, out var name) == false)
            name = $"<undefined:{(group.id).ToString()}>";

        return name;
    }
#else
    public static string ToName(this in ExclusiveGroupStruct group)
    {
        return ((uint)group.ToIDAndBitmask()).ToString();
    }
#endif
}