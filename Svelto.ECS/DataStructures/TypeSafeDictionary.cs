using System;
using System.Collections.Generic;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary
    {
        ITypeSafeDictionary Create();
        
        void RemoveEntitiesFromEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>
                                           entityViewEnginesDB, ref PlatformProfiler profiler);

        void MoveEntityFromDictionaryAndEngines(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup,
                                                  Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>
                                                      entityViewEnginesDB, ref PlatformProfiler profiler);
        
        void FillWithIndexedEntities(ITypeSafeDictionary entities);
        void AddEntitiesToEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
                                  ref PlatformProfiler profiler);
        
        void AddCapacity(int size);
        
        int Count { get; }
        void Trim();
        void Clear();
        bool Has(int entityIdEntityId);
    }

    class TypeSafeDictionary<TValue> : FasterDictionary<int, TValue>, ITypeSafeDictionary where TValue : IEntityStruct
    {
        public TypeSafeDictionary(int size):base(size)
        {}

        public TypeSafeDictionary()
        {}

        public void FillWithIndexedEntities(ITypeSafeDictionary entities)
        {
            int count;
            var buffer = (entities as TypeSafeDictionary<TValue>).GetValuesArray(out count);
            
            for (var i = 0; i < count; i++)
            {
                int idEntityId = 0;
                try
                {
                    idEntityId = buffer[i].ID.entityID;
                    
                    Add(idEntityId, ref buffer[i]);
                }
                catch (Exception e)
                {
                    throw new TypeSafeDictionaryException("trying to add an EntityView with the same ID more than once Entity: ".
                            FastConcat(typeof(TValue).ToString()).FastConcat("id ").FastConcat(idEntityId), e);
                }
            }
        }

        public void AddEntitiesToEngines(
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
            ref PlatformProfiler profiler)
        {
            var values = GetValuesArray(out var count);
            
            //pay attention: even if the entity is passed by ref, it won't be saved back in the database when this
            //function is called from the building of an entity. This is by design. Entity structs must be initialized
            //through the EntityInitializer method and not with an Add callback.
            //however the struct can be modified during an add callback if this happens as consequence of a group swap
            for (int i = 0; i < count; i++)
                AddEntityViewToEngines(entityViewEnginesDB, ref values[i], ref profiler);
        }

        public bool Has(int entityIdEntityId)
        {
            return ContainsKey(entityIdEntityId);
        }

        void AddEntityViewToEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
                                    ref TValue entity, ref PlatformProfiler profiler)
        {
            FasterList<IHandleEntityViewEngineAbstracted> entityViewsEngines;
            //get all the engines linked to TValue
            if (entityViewEnginesDB.TryGetValue(_type, out entityViewsEngines))
                for (int i = 0; i < entityViewsEngines.Count; i++)
                {
                    try
                    {
                        using (profiler.Sample((entityViewsEngines[i] as EngineInfo).name))
                        {
                            (entityViewsEngines[i] as IHandleEntityStructEngine<TValue>).AddInternal(ref entity);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ECSException("Code crashed inside Add callback ".
                                FastConcat(typeof(TValue).ToString()).FastConcat("id ").FastConcat(entity.ID.entityID), e);
                    }
                }
        }

        public void MoveEntityFromDictionaryAndEngines(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup,
                                                         Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>
                                                             entityViewEnginesDB, ref PlatformProfiler profiler)
        {
            int count;
            var fasterValuesBuffer = GetValuesArray(out count);
            var valueIndex = GetValueIndex(fromEntityGid.entityID);
            
            if (entityViewEnginesDB != null)
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref fasterValuesBuffer[valueIndex], ref profiler);

            if (toGroup != null)
            {
                var toGroupCasted = toGroup as TypeSafeDictionary<TValue>;
                fasterValuesBuffer[valueIndex].ID = toEntityID;
                toGroupCasted.Add(toEntityID.entityID, ref fasterValuesBuffer[valueIndex]);
                
                if (entityViewEnginesDB != null)
                    AddEntityViewToEngines(entityViewEnginesDB, ref toGroupCasted.GetValuesArray(out count)
                                               [toGroupCasted.GetValueIndex(toEntityID.entityID)], ref profiler);
            }

            Remove(fromEntityGid.entityID);
        }

        static void RemoveEntityViewFromEngines
        (Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB, ref TValue entity,
                        ref PlatformProfiler profiler)
        {
            FasterList<IHandleEntityViewEngineAbstracted> entityViewsEngines;
            if (entityViewEnginesDB.TryGetValue(_type, out entityViewsEngines))
                for (int i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample((entityViewsEngines[i] as EngineInfo).name, _typeName))
                        {
                            (entityViewsEngines[i] as IHandleEntityStructEngine<TValue>).RemoveInternal(ref entity);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ECSException("Code crashed inside Remove callback ".
                                  FastConcat(typeof(TValue).ToString()).FastConcat("id ").FastConcat(entity.ID.entityID), e);
                    }
        }
        
        public void RemoveEntitiesFromEngines(Dictionary<Type, 
                    FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB, ref PlatformProfiler profiler)
        {
            int count;
            TValue[] values = GetValuesArray(out count);

            for (int i = 0; i < count; i++)
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref values[i], ref profiler);
        }
        
        public ITypeSafeDictionary Create()
        {
            return new TypeSafeDictionary<TValue>();
        }
        
        public bool ExecuteOnEntityView<W>(int entityGidEntityId, ref W value, EntityAction<TValue, W> action)
        {
            uint findIndex;
            if (FindIndex(entityGidEntityId, out findIndex))
            {
                action(ref _values[findIndex], ref value);

                return true;
            }

            return false;
        }
        
        public bool ExecuteOnEntityView(int entityGidEntityId, EntityAction<TValue> action)
        {
            uint findIndex;
            if (FindIndex(entityGidEntityId, out findIndex))
            {
                action(ref _values[findIndex]);
                
                return true;
            }

            return false;
        }
        
        public uint FindElementIndex(int entityGidEntityId)
        {
            uint findIndex;
            if (FindIndex(entityGidEntityId, out findIndex) == false)
                throw new Exception("Entity not found in this group");

            return findIndex;
        }
        
        public bool TryFindElementIndex(int entityGidEntityId, out uint index)
        {
            return FindIndex(entityGidEntityId, out index);
        }
        
        static readonly Type _type = typeof(TValue);
        static readonly string _typeName = _type.Name;
    }
}