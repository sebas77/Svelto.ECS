using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Svelto.ECS.Serialization;

namespace Svelto.ECS
{
    static class GroupHashMap
    {
        /// <summary>
        /// c# Static constructors are guaranteed to be thread safe
        /// </summary>
         
        public static void Init()
        {
            List<Assembly> assemblies = AssemblyUtility.GetCompatibleAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    var typeOfExclusiveGroup = typeof(ExclusiveGroup);
                    var typeOfExclusiveGroupStruct     = typeof(ExclusiveGroupStruct);
                    
                    foreach (Type type in AssemblyUtility.GetTypesSafe(assembly))
                    {
                        if (type != null && type.IsClass && type.IsSealed && type.IsAbstract) //this means only static classes
                        {
                            var fields = type.GetFields();

                            foreach (var field in fields)
                            {
                                if (field.IsStatic)
                                {
                                    if (typeOfExclusiveGroup.IsAssignableFrom(field.FieldType))
                                    {
                                        var group = (ExclusiveGroup)field.GetValue(null);
                                        var name  = $"{type.FullName}.{field.Name}";
#if DEBUG                                        
                                        GroupNamesMap.idToName[(uint)@group] = $"{name} {(uint)group})";
#endif
                                        RegisterGroup(group, name);
                                    }
                                    else
                                    {
                                        if (typeOfExclusiveGroupStruct.IsAssignableFrom(field.FieldType))
                                        {
                                            var group = (ExclusiveGroupStruct)field.GetValue(null);
                                            var name  = $"{type.FullName}.{field.Name}";
#if DEBUG                                            
                                            GroupNamesMap.idToName[(uint)@group] = $"{name} {(uint)group})";
#endif
                                            RegisterGroup(@group, name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Console.LogDebugWarning(
                        "something went wrong while gathering group names on the assembly: ".FastConcat(assembly.FullName));
                }
            }
        }

        public static void RegisterGroup(ExclusiveGroupStruct exclusiveGroupStruct, string name)
        {
            //Group already registered by another field referencing the same group
            if (_hashByGroups.ContainsKey(exclusiveGroupStruct))
                return;
            
            var nameHash = DesignatedHash.Hash(Encoding.ASCII.GetBytes(name));
            
            if(_groupsByHash.ContainsKey(nameHash))
                throw new ECSException($"Group hash collision with {name} and {_groupsByHash[nameHash]}");
            
            Console.LogDebug($"Registering group {name} with ID {(uint)exclusiveGroupStruct} to {nameHash}");
            
            _groupsByHash.Add(nameHash, exclusiveGroupStruct);
            _hashByGroups.Add(exclusiveGroupStruct, nameHash);
        }
        
        public static uint GetHashFromGroup(ExclusiveGroupStruct groupStruct)
        {
#if DEBUG
            if (_hashByGroups.ContainsKey(groupStruct) == false)
                throw new ECSException($"Attempted to get hash from unregistered group {groupStruct}");
#endif
            
            return _hashByGroups[groupStruct];
        }
            
        public static ExclusiveGroupStruct GetGroupFromHash(uint groupHash)
        {
#if DEBUG
            if (_groupsByHash.ContainsKey(groupHash) == false)
                throw new ECSException($"Attempted to get group from unregistered hash {groupHash}");
#endif
            
            return _groupsByHash[groupHash];
        }

        static readonly Dictionary<uint, ExclusiveGroupStruct> _groupsByHash;
        static readonly Dictionary<ExclusiveGroupStruct, uint> _hashByGroups;

        static GroupHashMap()
        {
            _groupsByHash = new Dictionary<uint, ExclusiveGroupStruct>();
            _hashByGroups = new Dictionary<ExclusiveGroupStruct, uint>();
        }
    }
}