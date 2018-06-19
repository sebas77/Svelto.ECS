﻿using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;
using Svelto.Utilities;

namespace Svelto.ECS.Internal
{
    /// <summary>
    ///     This is just a place holder at the moment
    ///     I always wanted to create my own Dictionary
    ///     data structure as excercise, but never had the
    ///     time to. At the moment I need the custom interface
    ///     wrapped though.
    /// </summary>
    public interface ITypeSafeDictionary
    {
        ITypeSafeDictionary Create();
        
        void RemoveEntitiesFromEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>
                                           entityViewEnginesDB);

        void RemoveEntityFromEngines(EGID entityGid,
                                           Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>
                                               entityViewEnginesDB);
        
        void FillWithIndexedEntityViews(ITypeSafeDictionary                                          entityViews);
        void AddEntityViewsToEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB);
        
        void AddCapacity(int size);
        bool Remove(int idGid);
        
        int Count { get; }
    }

    class TypeSafeDictionary<TValue> : FasterDictionary<int, TValue>, ITypeSafeDictionary where TValue : IEntityStruct
    {
        public TypeSafeDictionary(int size):base(size)
        {}

        public TypeSafeDictionary()
        {}

        public void FillWithIndexedEntityViews(ITypeSafeDictionary entityViews)
        {
            int count;
            var buffer = (entityViews as TypeSafeDictionary<TValue>).GetFasterValuesBuffer(out count);

            try
            {
                for (var i = 0; i < count; i++)
                {
                    var entityView = buffer[i];

                    Add(entityView.ID.entityID, entityView);
                }
            }
            catch (Exception e)
            {
                throw new TypeSafeDictionaryException(e);
            }
        }

        public void AddEntityViewsToEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB)
        {
            int      count;
            TValue[] values = GetFasterValuesBuffer(out count);

            for (int i = 0; i < count; i++)
            {
                TValue entity = values[i];

                AddEntityViewToEngines(entityViewEnginesDB, ref entity);
            }
        }

        void AddEntityViewToEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB, ref TValue entity)
        {
            FasterList<IHandleEntityViewEngineAbstracted> entityViewsEngines;
            //get all the engines linked to TValue
            if (entityViewEnginesDB.TryGetValue(_type, out entityViewsEngines))
                for (int i = 0; i < entityViewsEngines.Count; i++)
                    (entityViewsEngines[i] as IHandleEntityStructEngine<TValue>).AddInternal(ref entity);
        }

        public void RemoveEntityFromEngines(EGID entityGid,
                                                  Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>
                                                      entityViewEnginesDB)
        {
            RemoveEntityViewFromEngines(entityViewEnginesDB, ref GetFasterValuesBuffer()[GetValueIndex(entityGid.entityID)]);
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
            TValue[] values = GetFasterValuesBuffer(out count);

            for (int i = 0; i < count; i++)
            {
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref values[i]);
            }
        }

        public ITypeSafeDictionary Create()
        {
            return new TypeSafeDictionary<TValue>();
        }
        
        public void ExecuteOnEntityView<W>(int entityGidEntityId, ref W value, ActionRef<TValue, W> action)
        {
            uint findIndex;
            if (FindIndex(entityGidEntityId, out findIndex))
            {
                action(ref _values[findIndex], ref value);
            }
        }
        
        public void ExecuteOnEntityView(int entityGidEntityId, ActionRef<TValue> action)
        {
            uint findIndex;
            if (FindIndex(entityGidEntityId, out findIndex))
            {
                action(ref _values[findIndex]);
            }
        }
        
        public uint FindElementIndex(int entityGidEntityId)
        {
            uint findIndex;
            if (FindIndex(entityGidEntityId, out findIndex) == false)
                throw new Exception("Entity not found in this group");

            return findIndex;
        }
        
        static readonly Type _type = typeof(TValue);
    }
}