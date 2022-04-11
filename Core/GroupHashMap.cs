using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Svelto.ECS.Serialization;

namespace Svelto.ECS
{
    public static class GroupHashMap
    {
        /// <summary>
        /// c# Static constructors are guaranteed to be thread safe
        /// </summary>
        internal static void Init()
        {
            List<Assembly> assemblies = AssemblyUtility.GetCompatibleAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    var typeOfExclusiveGroup       = typeof(ExclusiveGroup);
                    var typeOfExclusiveGroupStruct = typeof(ExclusiveGroupStruct);
                    var typeOfExclusiveBuildGroup  = typeof(ExclusiveBuildGroup);

                    foreach (Type type in AssemblyUtility.GetTypesSafe(assembly))
                    {
                        CheckForGroupCompounds(type);

                        if (type != null && type.IsClass && type.IsSealed &&
                            type.IsAbstract) //IsClass and IsSealed and IsAbstract means only static classes
                        {
                            var subClasses = type.GetNestedTypes();

                            foreach (var subclass in subClasses)
                            {
                                CheckForGroupCompounds(subclass);
                            }

                            var fields = type.GetFields();

                            foreach (var field in fields)
                            {
                                if (field.IsStatic 
                                     && (typeOfExclusiveGroup.IsAssignableFrom(field.FieldType) 
                                     || typeOfExclusiveGroupStruct.IsAssignableFrom(field.FieldType) 
                                     || typeOfExclusiveBuildGroup.IsAssignableFrom(field.FieldType)))
                                {
                                    uint groupID;

                                    if (typeOfExclusiveGroup.IsAssignableFrom(field.FieldType))
                                    {
                                        var group = (ExclusiveGroup)field.GetValue(null);
                                        groupID = ((ExclusiveGroupStruct)@group).id;
                                    }
                                    else
                                    if (typeOfExclusiveGroupStruct.IsAssignableFrom(field.FieldType))
                                    {
                                        var group = (ExclusiveGroupStruct)field.GetValue(null);
                                        groupID = @group.id;
                                    }
                                    else
                                    {
                                        var group = (ExclusiveBuildGroup)field.GetValue(null);
                                        groupID = ((ExclusiveGroupStruct)@group).id;
                                    }

                                    {
                                        ExclusiveGroupStruct group = new ExclusiveGroupStruct(groupID);
#if DEBUG && !PROFILE_SVELTO
                                        if (GroupNamesMap.idToName.ContainsKey(@group) == false)
                                            GroupNamesMap.idToName[@group] =
                                                $"{type.FullName}.{field.Name} {@group.id})";
#endif
                                        //The hashname is independent from the actual group ID. this is fundamental because it is want
                                        //guarantees the hash to be the same across different machines
                                        RegisterGroup(@group, $"{type.FullName}.{field.Name}");
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Console.LogDebugWarning(
                        "something went wrong while gathering group names on the assembly: ".FastConcat(
                            assembly.FullName));
                }
            }
        }

        static void CheckForGroupCompounds(Type type)
        {
            if (typeof(ITouchedByReflection).IsAssignableFrom(type))
            {
                //this wil call the static constructor, but only once. Static constructors won't be called
                //more than once with this
                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.BaseType.TypeHandle);
            }
        }

        /// <summary>
        /// The hashname is independent from the actual group ID. this is fundamental because it is want
        /// guarantees the hash to be the same across different machines
        /// </summary>
        /// <param name="exclusiveGroupStruct"></param>
        /// <param name="name"></param>
        /// <exception cref="ECSException"></exception>
        public static void RegisterGroup(ExclusiveGroupStruct exclusiveGroupStruct, string name)
        {
            //Group already registered by another field referencing the same group, can happen because
            //the group poked is a group compound which static constructor is already been called at this point
            if (_hashByGroups.ContainsKey(exclusiveGroupStruct))
                return;

            var nameHash = DesignatedHash.Hash(Encoding.ASCII.GetBytes(name));

            if (_groupsByHash.ContainsKey(nameHash))
                throw new ECSException($"Group hash collision with {name} and {_groupsByHash[nameHash]}");

            Console.LogDebug($"Registering group {name} with ID {exclusiveGroupStruct.id} to {nameHash}");

            _groupsByHash.Add(nameHash, exclusiveGroupStruct);
            _hashByGroups.Add(exclusiveGroupStruct, nameHash);
        }

        internal static uint GetHashFromGroup(ExclusiveGroupStruct groupStruct)
        {
#if DEBUG && !PROFILE_SVELTO
            if (_hashByGroups.ContainsKey(groupStruct) == false)
                throw new ECSException($"Attempted to get hash from unregistered group {groupStruct}");
#endif

            return _hashByGroups[groupStruct];
        }

        internal static ExclusiveGroupStruct GetGroupFromHash(uint groupHash)
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