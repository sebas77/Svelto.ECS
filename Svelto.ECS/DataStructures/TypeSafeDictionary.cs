using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.Common;
 using Svelto.Common.Internal;
 using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary
    {
        int                 Count { get; }
        ITypeSafeDictionary Create();

        void RemoveEntitiesFromEngines(
            Dictionary<Type, FasterList<IEngine>> entityViewEnginesDB,
            ref PlatformProfiler profiler);

        void MoveEntityFromDictionaryAndEngines(EGID fromEntityGid, EGID? toEntityID, ITypeSafeDictionary toGroup,
            Dictionary<Type, FasterList<IEngine>> engines,
            ref PlatformProfiler profiler);

        void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId);

        void AddEntitiesToEngines(Dictionary<Type, FasterList<IEngine>> entityViewEnginesDb,
            ITypeSafeDictionary realDic, ref PlatformProfiler profiler);

        void SetCapacity(uint size);
        void Trim();
        void Clear();
        bool Has(uint entityIdEntityId);
    }

    class TypeSafeDictionary<TValue> : FasterDictionary<uint, TValue>, ITypeSafeDictionary where TValue : struct, IEntityStruct
    {
        static readonly Type   _type     = typeof(TValue);
        static readonly string _typeName = _type.Name;
        static readonly bool   _hasEgid   = typeof(INeedEGID).IsAssignableFrom(_type);
        
        public TypeSafeDictionary(uint size) : base(size) { }
        public TypeSafeDictionary() {}
        
        public void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId)
        {
            var typeSafeDictionary = entitiesToSubmit as TypeSafeDictionary<TValue>;
            
            foreach (var tuple in typeSafeDictionary)
            {
                try
                {
                    if (_hasEgid)
                    {
                        var needEgid = (INeedEGID)tuple.Value;
                        needEgid.ID = new EGID(tuple.Key, groupId);
                        
                        Add(tuple.Key, (TValue) needEgid);
                    }
                    else
                        Add(tuple.Key, ref tuple.Value);
                }
                catch (Exception e)
                {
                    throw new TypeSafeDictionaryException(
                        "trying to add an EntityView with the same ID more than once Entity: "
                           .FastConcat(typeof(TValue).ToString()).FastConcat("id ").FastConcat(tuple.Key), e);
                }
            }
        }

        public void AddEntitiesToEngines(
            Dictionary<Type, FasterList<IEngine>> entityViewEnginesDB,
            ITypeSafeDictionary realDic, ref PlatformProfiler profiler)
        {
            foreach (var value in this)
            {
                var typeSafeDictionary = realDic as TypeSafeDictionary<TValue>;
               
                AddEntityViewToEngines(entityViewEnginesDB, ref typeSafeDictionary.GetValueByRef(value.Key), null,
                                       ref profiler);
            }
        }

        public bool Has(uint entityIdEntityId) { return ContainsKey(entityIdEntityId); }

        public void MoveEntityFromDictionaryAndEngines(EGID fromEntityGid, EGID? toEntityID,
            ITypeSafeDictionary toGroup,
            Dictionary<Type, FasterList<IEngine>> engines,
            ref PlatformProfiler profiler)
        {
            var valueIndex = GetValueIndex(fromEntityGid.entityID);

            if (toGroup != null)
            {
                RemoveEntityViewFromEngines(engines, ref _values[valueIndex], fromEntityGid.groupID, ref profiler);
                
                var toGroupCasted = toGroup as TypeSafeDictionary<TValue>;
                ref var entity = ref _values[valueIndex];
                var previousGroup = fromEntityGid.groupID;
                
                ///
                /// NOTE I WOULD EVENTUALLY NEED TO REUSE THE REAL ID OF THE REMOVING ELEMENT
                /// SO THAT I CAN DECREASE THE GLOBAL GROUP COUNT
                /// 
                
          //      entity.ID = EGID.UPDATE_REAL_ID_AND_GROUP(entity.ID, toEntityID.groupID, entityCount);
                  if (_hasEgid)
                  {
                      var needEgid = (INeedEGID)entity;
                      needEgid.ID = toEntityID.Value;
                      entity = (TValue) needEgid;
                  }
                
                var index = toGroupCasted.Add(fromEntityGid.entityID, ref entity);

                 AddEntityViewToEngines(engines, ref toGroupCasted._values[index], previousGroup,
                                           ref profiler);
            }
            else
                RemoveEntityViewFromEngines(engines, ref _values[valueIndex], null, ref profiler);


             Remove(fromEntityGid.entityID);
        }

        public void RemoveEntitiesFromEngines(
            Dictionary<Type, FasterList<IEngine>> entityViewEnginesDB,
            ref PlatformProfiler profiler)
        {
            var values = GetValuesArray(out var count);

            for (var i = 0; i < count; i++)
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref values[i], null, ref profiler);
        }

        public ITypeSafeDictionary Create() { return new TypeSafeDictionary<TValue>(); }

        void AddEntityViewToEngines(Dictionary<Type, FasterList<IEngine>> entityViewEnginesDB,
                                    ref TValue                                                      entity,
                                    ExclusiveGroup.ExclusiveGroupStruct?                            previousGroup,
                                    ref PlatformProfiler                                            profiler)
        {
            //get all the engines linked to TValue
            if (!entityViewEnginesDB.TryGetValue(_type, out var entityViewsEngines)) return;

            if (previousGroup == null)
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                        {
                            (entityViewsEngines[i] as IReactOnAddAndRemove<TValue>).Add(ref entity);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Add callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
            else
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                        {
                            (entityViewsEngines[i] as IReactOnSwap<TValue>).MovedTo(ref entity, previousGroup.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Add callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
        }

        static void RemoveEntityViewFromEngines(
            Dictionary<Type, FasterList<IEngine>> entityViewEnginesDB, ref TValue entity,
            ExclusiveGroup.ExclusiveGroupStruct?                            previousGroup,
            ref PlatformProfiler                                            profiler)
        {
            if (!entityViewEnginesDB.TryGetValue(_type, out var entityViewsEngines)) return;

            if (previousGroup == null)
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                            (entityViewsEngines[i] as IReactOnAddAndRemove<TValue>).Remove(ref entity);
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
            else
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                            (entityViewsEngines[i] as IReactOnSwap<TValue>).MovedFrom(ref entity);
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref TValue FindElement(uint entityGidEntityId)
        {
#if DEBUG && !PROFILER         
            if (TryFindIndex(entityGidEntityId, out var findIndex) == false)
                throw new Exception("Entity not found in this group ".FastConcat(typeof(TValue).ToString()));
#else
            TryFindIndex(entityGidEntityId, out var findIndex);
#endif
            return ref _values[findIndex];
        }
    }
}