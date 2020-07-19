using System;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        internal FasterDictionary<uint, ITypeSafeDictionary> FindGroups_INTERNAL<T1>() where T1 : IEntityComponent
        {
            if (_groupsPerEntity.ContainsKey(TypeRefWrapper<T1>.wrapper) == false)
                return _emptyDictionary;

            return _groupsPerEntity[TypeRefWrapper<T1>.wrapper];
        }
        
        public LocalFasterReadOnlyList<ExclusiveGroupStruct> FindGroups<T1>() where T1 : IEntityComponent
        {
            FasterList<ExclusiveGroupStruct> result = groups.Value;
            result.FastClear();
            if (_groupsPerEntity.TryGetValue(TypeRefWrapper<T1>.wrapper
                                           , out FasterDictionary<uint, ITypeSafeDictionary> result1) == false)
                return result;
        
            var result1Count           = result1.count;
            var fasterDictionaryNodes1 = result1.unsafeKeys;
        
            for (int j = 0; j < result1Count; j++)
            {
                result.Add(new ExclusiveGroupStruct(fasterDictionaryNodes1[j].key));
            }
        
            return result;
        }
        
        public LocalFasterReadOnlyList<ExclusiveGroupStruct> FindGroups<T1, T2>() where T1 : IEntityComponent  where T2 : IEntityComponent
        {
            FasterList<ExclusiveGroupStruct> result = groups.Value;
            result.FastClear();
            if (_groupsPerEntity.TryGetValue(TypeRefWrapper<T1>.wrapper
                                           , out FasterDictionary<uint, ITypeSafeDictionary> result1) == false)
                return result;
            if (_groupsPerEntity.TryGetValue(TypeRefWrapper<T2>.wrapper
                                           , out FasterDictionary<uint, ITypeSafeDictionary> result2) == false)
                return result;
            
            var result1Count           = result1.count;
            var result2Count           = result2.count;
            var fasterDictionaryNodes1 = result1.unsafeKeys;
            var fasterDictionaryNodes2 = result2.unsafeKeys;

            for (int i = 0; i < result1Count; i++)
            {
                for (int j = 0; j < result2Count; j++)
                {
                    //if the same group is found used with both T1 and T2
                    if (fasterDictionaryNodes1[i].key == fasterDictionaryNodes2[j].key)
                    {
                        result.Add(new ExclusiveGroupStruct(fasterDictionaryNodes1[i].key));
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Remember that this operation os O(N*M*P) where N,M,P are the number of groups where each component
        /// is found.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <returns></returns>
        public LocalFasterReadOnlyList<ExclusiveGroupStruct> FindGroups<T1, T2, T3>()
            where T1 : IEntityComponent where T2 : IEntityComponent where T3 : IEntityComponent
        {
            FindGroups<T1, T2>();
            
            FasterList<ExclusiveGroupStruct> result = groups.Value;

            if (result.count == 0)
                return result;
            
            if (_groupsPerEntity.TryGetValue(TypeRefWrapper<T3>.wrapper
                                           , out FasterDictionary<uint, ITypeSafeDictionary> result3) == false)
                return result;

            var result3Count           = result3.count;
            var fasterDictionaryNodes3 = result3.unsafeKeys;

            for (int j = 0; j < result3Count; j++)
            for (int i = (int) 0; i < result.count; i++)
            {
                if (fasterDictionaryNodes3[j].key == result[i])
                    break;

                result.UnorderedRemoveAt(i);
                i--;
            }

            return result;
        }
        
        struct GroupsList
        {
            static GroupsList()
            {
                groups = new FasterList<ExclusiveGroupStruct>();
            }

            static readonly FasterList<ExclusiveGroupStruct> groups;

            public static implicit operator FasterList<ExclusiveGroupStruct>(in GroupsList list)
            {
                return list.reference;
            }

            FasterList<ExclusiveGroupStruct> reference => groups;
        }
        
        static readonly ThreadLocal<GroupsList> groups = new ThreadLocal<GroupsList>();
    }
}