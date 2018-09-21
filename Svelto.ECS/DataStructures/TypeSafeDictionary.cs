using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary
    {
        ITypeSafeDictionary Create();
        
        void RemoveEntitiesFromEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>
                                           entityViewEnginesDB);

        void MoveEntityFromDictionaryAndEngines(EGID fromEntityGid, int toGroupID, ITypeSafeDictionary toGroup,
                                                  Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>
                                                      entityViewEnginesDB);
        
        void FillWithIndexedEntities(ITypeSafeDictionary entities);
        void AddEntitiesToEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB);
        
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

            try
            {
                for (var i = 0; i < count; i++)
                {
                    Add(buffer[i].ID.entityID, ref buffer[i]);
                }
            }
            catch (Exception e)
            {
                throw new TypeSafeDictionaryException(e);
            }
        }

        public void AddEntitiesToEngines(
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB)
        {
            int      count;
            TValue[] values = GetValuesArray(out count);

            for (int i = 0; i < count; i++)
            {
                TValue entity = values[i];

                AddEntityViewToEngines(entityViewEnginesDB, ref entity);
            }
        }

        public bool Has(int entityIdEntityId)
        {
            return ContainsKey(entityIdEntityId);
        }

        public int GetFirstID()
        {
            return Values[0].ID.entityID;
        }

        void AddEntityViewToEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB,
                                    ref TValue                                                      entity)
        {
            FasterList<IHandleEntityViewEngineAbstracted> entityViewsEngines;
            //get all the engines linked to TValue
            if (entityViewEnginesDB.TryGetValue(_type, out entityViewsEngines))
                for (int i = 0; i < entityViewsEngines.Count; i++)
                    (entityViewsEngines[i] as IHandleEntityStructEngine<TValue>).AddInternal(ref entity);
        }

        public void MoveEntityFromDictionaryAndEngines(EGID fromEntityGid, int toGroupID, ITypeSafeDictionary toGroup,
                                                         Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>
                                                             entityViewEnginesDB)
        {
            int count;
            var fasterValuesBuffer = GetValuesArray(out count);
            var valueIndex = GetValueIndex(fromEntityGid.entityID);

            if (entityViewEnginesDB != null)
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref fasterValuesBuffer[valueIndex]);

            if (toGroup != null)
            {
                var toGroupCasted = toGroup as TypeSafeDictionary<TValue>;
                fasterValuesBuffer[valueIndex].ID = new EGID(fromEntityGid.entityID, toGroupID);
                toGroupCasted.Add(fromEntityGid.entityID, ref fasterValuesBuffer[valueIndex]);
                
                if (entityViewEnginesDB != null)
                    AddEntityViewToEngines(entityViewEnginesDB, ref toGroupCasted.GetValuesArray(out count)[toGroupCasted.GetValueIndex(fromEntityGid.entityID)]);
            }

            Remove(fromEntityGid.entityID);
        }

        static void RemoveEntityViewFromEngines
            (Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB, ref TValue entity)
        {
            FasterList<IHandleEntityViewEngineAbstracted> entityViewsEngines;
            if (entityViewEnginesDB.TryGetValue(_type, out entityViewsEngines))
                for (int i = 0; i < entityViewsEngines.Count; i++)
                    (entityViewsEngines[i] as IHandleEntityStructEngine<TValue>).RemoveInternal(ref entity);
        }
        
        public void RemoveEntitiesFromEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB)
        {
            int count;
            TValue[] values = GetValuesArray(out count);

            for (int i = 0; i < count; i++)
            {
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref values[i]);
            }
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
    }
}