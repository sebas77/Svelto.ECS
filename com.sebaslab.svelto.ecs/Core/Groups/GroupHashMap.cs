using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Svelto.ECS.Serialization;

namespace Svelto.ECS
{
    public static class GroupHashMap
    {
        /// <summary>
        /// c# Static constructors are guaranteed to be thread safe
        /// The runtime guarantees that a static constructor is only called once. So even if a type is called by multiple threads at the same time,
        /// the static constructor is always executed one time. To get a better understanding how this works, it helps to know what purpose it serves.
        ///
        /// Warmup the group hash map. This will call all the static constructors of the group types
        /// </summary>
        internal static void WarmUp()
        {
            List<Assembly> assemblies = AssemblyUtility.GetCompatibleAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                var typeOfExclusiveGroup = typeof(ExclusiveGroup);
                var typeOfExclusiveGroupStruct = typeof(ExclusiveGroupStruct);
                var typeOfExclusiveBuildGroup = typeof(ExclusiveBuildGroup);

                var typesSafe = AssemblyUtility.GetTypesSafe(assembly);
                foreach (Type type in typesSafe)
                {
                    CheckForGroupCompounds(type);

                    //Search inside static types
                    if (type != null && type.IsClass && type.IsSealed
                     && type.IsAbstract) //IsClass and IsSealed and IsAbstract means only static classes
                    {
                        var subClasses = type.GetNestedTypes();

                        foreach (var subclass in subClasses)
                        {
                            CheckForGroupCompounds(subclass);
                        }

                        var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                        foreach (var field in fields)
                        {
                            if ((typeOfExclusiveGroup.IsAssignableFrom(field.FieldType)
                                 || typeOfExclusiveGroupStruct.IsAssignableFrom(field.FieldType)
                                 || typeOfExclusiveBuildGroup.IsAssignableFrom(field.FieldType)))
                            {
                                uint groupIDAndBitMask;

                                int range = 0;
                                if (typeOfExclusiveGroup.IsAssignableFrom(field.FieldType))
                                {
                                    var group = (ExclusiveGroup)field.GetValue(null);
                                    groupIDAndBitMask = ((ExclusiveGroupStruct)@group).ToIDAndBitmask();
                                    range = @group._range;
                                }
                                else if (typeOfExclusiveGroupStruct.IsAssignableFrom(field.FieldType))
                                {
                                    var group = (ExclusiveGroupStruct)field.GetValue(null);
                                    groupIDAndBitMask = @group.ToIDAndBitmask();
                                }
                                else
                                {
                                    var group = (ExclusiveBuildGroup)field.GetValue(null);
                                    groupIDAndBitMask = ((ExclusiveGroupStruct)@group).ToIDAndBitmask();
                                }

                                {
                                    var bitMask = (byte)(groupIDAndBitMask >> 24);
                                    var groupID = groupIDAndBitMask & 0xFFFFFF;
                                    ExclusiveGroupStruct group = new ExclusiveGroupStruct(groupID, bitMask);
#if DEBUG && !PROFILE_SVELTO
                                    if (GroupNamesMap.idToName.ContainsKey(@group) == false)
                                        GroupNamesMap.idToName[@group] =
                                                $"{type.FullName}.{field.Name} id: {@group.id}";
#endif
                                    //The hashname is independent from the actual group ID. this is fundamental because it is want
                                    //guarantees the hash to be the same across different machines
                                    RegisterGroup(@group, $"{type.FullName}.{field.Name}");

                                    for (uint i = 1; i < range; i++)
                                    {
                                        var exclusiveGroupStruct = group + i;
#if DEBUG && !PROFILE_SVELTO                                        
                                        if (GroupNamesMap.idToName.ContainsKey(exclusiveGroupStruct) == false)
                                            GroupNamesMap.idToName[exclusiveGroupStruct] =
                                                    $"{type.FullName}.{field.Name} id: {@group.id + i}";
#endif
                                        RegisterGroup(exclusiveGroupStruct, $"{type.FullName}.{field.Name} id: {@group.id + i}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void CheckForGroupCompounds(Type type)
        {
            if (typeof(ITouchedByReflection).IsAssignableFrom(type))
            {
                //this calls the static constructor, but only once. Static constructors won't be called
                //more than once with this
                CallStaticConstructorsRecursively(type);
            }

            static void CallStaticConstructorsRecursively(Type type)
            {
                // Check if the current type has a static constructor
//                type.TypeInitializer.Invoke(null, null); //calling Invoke will force the static constructor to be called even if already called, this is a problem because GroupTag and Compound throw an exception if called multiple times
                RuntimeHelpers.RunClassConstructor(type.TypeHandle); //this will call the static constructor only once

                // Recursively check the base types
                Type baseType = type.BaseType;
                if (baseType != null && baseType != typeof(object)) //second if means we got the the end of the hierarchy
                {
                    CallStaticConstructorsRecursively(baseType);
                }
            }
        }

        /// <summary>
        /// The hashname is independent from the actual group ID. this is fundamental because it
        /// guarantees the hash to be the same across different machines
        /// </summary>
        /// <param name="exclusiveGroupStruct"></param>
        /// <param name="name"></param>
        /// <exception cref="ECSException"></exception>
        internal static void RegisterGroup(ExclusiveGroupStruct exclusiveGroupStruct, string name)
        {
            //Group already registered by another field referencing the same group, can happen because
            //the group poked is a group compound which static constructor is already been called at this point
            if (_hashByGroups.ContainsKey(exclusiveGroupStruct))
                return;

            var nameHash = DesignatedHash.Hash(Encoding.ASCII.GetBytes(name));

            if (_groupsByHash.TryGetValue(nameHash, out var value))
                throw new ECSException($"Group hash collision with {name} and {value}");

            Console.LogDebug($"Registering group {name} with ID {exclusiveGroupStruct.id} to {nameHash}");

            _groupsByHash.Add(nameHash, exclusiveGroupStruct);
            _hashByGroups.Add(exclusiveGroupStruct, nameHash);
        }

        public static uint GetHashFromGroup(ExclusiveGroupStruct groupStruct)
        {
#if DEBUG && !PROFILE_SVELTO
            if (_hashByGroups.ContainsKey(groupStruct) == false)
                throw new ECSException($"Attempted to get hash from unregistered group {groupStruct}");
#endif

            return _hashByGroups[groupStruct];
        }

        public static ExclusiveGroupStruct GetGroupFromHash(uint groupHash)
        {
#if DEBUG && !PROFILE_SVELTO
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