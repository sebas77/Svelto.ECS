using Svelto.ECS;

#if DEBUG
using System;
using System.Collections.Generic;
using System.Reflection;

public static class ExclusiveGroupDebugger
{
    static ExclusiveGroupDebugger()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (Assembly assembly in assemblies)
        {
            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                if (type != null && type.IsClass && type.IsSealed && type.IsAbstract) //this means only static classes
                {
                    var fields = type.GetFields();
                    foreach (var field in fields)
                    {
                        if (field.IsStatic && typeof(ExclusiveGroup).IsAssignableFrom(field.FieldType))
                        {
                            string name  = $"{type.FullName}.{field.Name}";
                            var    group = (ExclusiveGroup) field.GetValue(null);
                            GroupMap.idToName[(ExclusiveGroupStruct) group] = name;
                        }

                        if (field.IsStatic && typeof(ExclusiveGroupStruct).IsAssignableFrom(field.FieldType))
                        {
                            string name  = $"{type.FullName}.{field.Name}";
                            var    group = (ExclusiveGroupStruct) field.GetValue(null);
                            GroupMap.idToName[@group] = name;
                        }
                    }
                }
            }
        }
    }
    
    public static string ToName(this in ExclusiveGroupStruct group)
    {
        if (GroupMap.idToName.TryGetValue(group, out var name) == false)
            name = $"<undefined:{((uint)group).ToString()}>";

        return name;
    }
}

public static class GroupMap
{
    static GroupMap()
    {
        GroupMap.idToName = new Dictionary<uint, string>();
    }

    internal static readonly Dictionary<uint, string> idToName;
}
#else
public static class ExclusiveGroupDebugger
{
    public static string ToName(this in ExclusiveGroupStruct group)
    {
        return ((uint)group).ToString();
    }
}
#endif