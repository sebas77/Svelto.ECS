using System.Collections.Generic;
using Svelto.ECS;

static class GroupNamesMap
{
#if DEBUG
    static GroupNamesMap() { idToName = new Dictionary<uint, string>(); }

    internal static readonly Dictionary<uint, string> idToName;
#endif
#if DEBUG
    public static string ToName(this in ExclusiveGroupStruct group)
    {
        var idToName = GroupNamesMap.idToName;
        if (idToName.TryGetValue((uint)@group, out var name) == false)
            name = $"<undefined:{((uint)group).ToString()}>";

        return name;
    }
#else
    public static string ToName(this in ExclusiveGroupStruct group)
    {
        return ((uint)group).ToString();
    }
#endif
}