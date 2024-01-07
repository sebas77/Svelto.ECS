using System;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        /// <summary>
        /// Return all the groups where the entity component is found. It's a linear operation, but usually the number of groups are very low
        /// By default Groups marked as disabled are not returned, however this can be overridden using the ignoreDisabledBit parameter
        /// </summary>
        public LocalFasterReadOnlyList<ExclusiveGroupStruct> FindGroups<T1>(bool ignoreDisabledBit = false) where T1 : struct, _IInternalEntityComponent
        {
            FasterList<ExclusiveGroupStruct> result = localgroups.Value.groupArray;
            result.Clear();
            if (groupsPerComponent.TryGetValue(ComponentTypeID<T1>.id
                                          , out FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> result1)
             == false)
                return result;

            var result1Count           = result1.count;
            var fasterDictionaryNodes1 = result1.unsafeKeys;

            for (int j = 0; j < result1Count; j++)
            {
                var group = fasterDictionaryNodes1[j].key;
                if (ignoreDisabledBit == false && group.IsEnabled() == false) continue;
                
                result.Add(group);
            }

            return result;
        }

        public LocalFasterReadOnlyList<ExclusiveGroupStruct> FindGroups<T1, T2>(bool ignoreDisabledBit = false)
            where T1 : struct, _IInternalEntityComponent where T2 : struct, _IInternalEntityComponent
        {
            FasterList<ExclusiveGroupStruct> result = localgroups.Value.groupArray;
            result.Clear();
            if (groupsPerComponent.TryGetValue(ComponentTypeID<T1>.id
                                          , out FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> result1)
             == false)
                return result;
            if (groupsPerComponent.TryGetValue(ComponentTypeID<T2>.id
                                          , out FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> result2)
             == false)
                return result;

            var result1Count           = result1.count;
            var result2Count           = result2.count;
            var fasterDictionaryNodes1 = result1.unsafeKeys;
            var fasterDictionaryNodes2 = result2.unsafeKeys;

            for (int i = 0; i < result1Count; i++)
            {
                var groupID = fasterDictionaryNodes1[i].key;
                if (ignoreDisabledBit == false && groupID.IsEnabled() == false) continue;

                for (int j = 0; j < result2Count; j++)
                {
                    //if the same group is found used with both T1 and T2
                    if (groupID == fasterDictionaryNodes2[j].key)
                    {
                        result.Add(groupID);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Remember that this operation os O(N*M*P) where N,M,P are the number of groups where each component
        /// is found.
        /// TODO: I have to find once for ever a solution to be sure that the entities in the groups match
        /// Currently this returns group where the entities are found, but the entities may not match in these
        /// groups.
        /// Checking the size of the entities is an early check, needed, but not sufficient, as entities components may
        /// coincidentally match in number but not from which entities they are generated
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns></returns>
        public LocalFasterReadOnlyList<ExclusiveGroupStruct> FindGroups<T1, T2, T3>(bool ignoreDisabledBit = false)
            where T1 : struct, _IInternalEntityComponent where T2 : struct, _IInternalEntityComponent where T3 : struct, _IInternalEntityComponent
        {
            FasterList<FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>> localArray =
                localgroups.Value.listOfGroups;

            if (groupsPerComponent.TryGetValue(ComponentTypeID<T1>.id, out localArray[0]) == false || localArray[0].count == 0)
                return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(
                    FasterReadOnlyList<ExclusiveGroupStruct>.DefaultEmptyList);
            if (groupsPerComponent.TryGetValue(ComponentTypeID<T2>.id, out localArray[1]) == false || localArray[1].count == 0)
                return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(
                    FasterReadOnlyList<ExclusiveGroupStruct>.DefaultEmptyList);
            if (groupsPerComponent.TryGetValue(ComponentTypeID<T3>.id, out localArray[2]) == false || localArray[2].count == 0)
                return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(
                    FasterReadOnlyList<ExclusiveGroupStruct>.DefaultEmptyList);

            localgroups.Value.groups.Clear();

            FasterDictionary<ExclusiveGroupStruct, ExclusiveGroupStruct> localGroups = localgroups.Value.groups;

            int startIndex = 0;
            int min        = int.MaxValue;

            for (int i = 0; i < 3; i++)
                if (localArray[i].count < min)
                {
                    min        = localArray[i].count;
                    startIndex = i;
                }

            foreach (var value in localArray[startIndex])
            {
                if (ignoreDisabledBit == false && value.key.IsEnabled() == false) continue;
                
                localGroups.Add(value.key, value.key);
            }

            var groupData = localArray[++startIndex % 3];
            localGroups.Intersect(groupData);
            if (localGroups.count == 0)
                return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(
                    FasterReadOnlyList<ExclusiveGroupStruct>.DefaultEmptyList);
            
            groupData = localArray[++startIndex % 3];
            localGroups.Intersect(groupData);

            return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(localGroups.unsafeValues
                                                                   , (uint) localGroups.count);
        }

        public LocalFasterReadOnlyList<ExclusiveGroupStruct> FindGroups<T1, T2, T3, T4>(bool ignoreDisabledBit = false)
            where T1 : struct, _IInternalEntityComponent
            where T2 : struct, _IInternalEntityComponent
            where T3 : struct, _IInternalEntityComponent
            where T4 : struct, _IInternalEntityComponent
        {
            FasterList<FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>> localArray =
                localgroups.Value.listOfGroups;

            if (groupsPerComponent.TryGetValue(ComponentTypeID<T1>.id, out localArray[0]) == false || localArray[0].count == 0)
                return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(
                    FasterReadOnlyList<ExclusiveGroupStruct>.DefaultEmptyList);
            if (groupsPerComponent.TryGetValue(ComponentTypeID<T2>.id, out localArray[1]) == false || localArray[1].count == 0)
                return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(
                    FasterReadOnlyList<ExclusiveGroupStruct>.DefaultEmptyList);
            if (groupsPerComponent.TryGetValue(ComponentTypeID<T3>.id, out localArray[2]) == false || localArray[2].count == 0)
                return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(
                    FasterReadOnlyList<ExclusiveGroupStruct>.DefaultEmptyList);
            if (groupsPerComponent.TryGetValue(ComponentTypeID<T4>.id, out localArray[3]) == false || localArray[3].count == 0)
                return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(
                    FasterReadOnlyList<ExclusiveGroupStruct>.DefaultEmptyList);

            localgroups.Value.groups.Clear();

            var localGroups = localgroups.Value.groups;

            int startIndex = 0;
            int min        = int.MaxValue;

            for (int i = 0; i < 4; i++)
            {
                var fasterDictionary = localArray[i];
                if (fasterDictionary.count < min)
                {
                    min        = fasterDictionary.count;
                    startIndex = i;
                }
            }

            foreach (var value in localArray[startIndex])
            {
                if (ignoreDisabledBit == false && value.key.IsEnabled() == false) continue;
                localGroups.Add(value.key, value.key);
            }

            var groupData = localArray[++startIndex & 3]; //&3 == %4
            localGroups.Intersect(groupData);
            if (localGroups.count == 0)
                return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(
                    FasterReadOnlyList<ExclusiveGroupStruct>.DefaultEmptyList);
            
            groupData = localArray[++startIndex & 3];
            localGroups.Intersect(groupData);
            if (localGroups.count == 0)
                return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(
                    FasterReadOnlyList<ExclusiveGroupStruct>.DefaultEmptyList);
            
            groupData = localArray[++startIndex & 3];
            localGroups.Intersect(groupData);

            return new LocalFasterReadOnlyList<ExclusiveGroupStruct>(localGroups.unsafeValues
                                                                   , (uint) localGroups.count);
        }

        internal FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> FindGroups_INTERNAL(ComponentID type)
        {
            if (groupsPerComponent.ContainsKey(type) == false)
                return _emptyDictionary;

            return groupsPerComponent[type];
        }

        struct GroupsList
        {
            internal FasterDictionary<ExclusiveGroupStruct, ExclusiveGroupStruct>            groups;
            internal FasterList<FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>> listOfGroups;
            public   FasterList<ExclusiveGroupStruct>                                        groupArray;
        }

        static readonly ThreadLocal<GroupsList> localgroups = new ThreadLocal<GroupsList>(() =>
        {
            GroupsList gl = default;

            gl.groups       = new FasterDictionary<ExclusiveGroupStruct, ExclusiveGroupStruct>();
            gl.listOfGroups = FasterList<FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>>.PreInit(4);
            gl.groupArray   = new FasterList<ExclusiveGroupStruct>(1);

            return gl;
        });
    }
}