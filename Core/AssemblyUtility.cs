using System;
using System.Collections.Generic;
using System.Reflection;

public static class AssemblyUtility
{
    static readonly List<Assembly> AssemblyList = new List<Assembly>();
    
    static AssemblyUtility()
    {
        var        assemblyName = Assembly.GetExecutingAssembly().GetName();
        Assembly[] assemblies   = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        { 
            AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
            if (Array.Exists(referencedAssemblies, (a) => a.Name == assemblyName.Name))
            {
                AssemblyList.Add(assembly);
            }
        }
    }

    public static IEnumerable<Type> GetTypesSafe(Assembly assembly)
    {
        try
        {
            Type[] types = assembly.GetTypes();

            return types;
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types;
        }
    }

    public static List<Assembly> GetCompatibleAssemblies() { return AssemblyList; }
}