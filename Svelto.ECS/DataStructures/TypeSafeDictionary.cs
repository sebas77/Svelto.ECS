﻿using System;
using System.Collections.Generic;
using Svelto.DataStructures;

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
        void RemoveEntityFromDicAndEngines(EGID entityGid,
                                           Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>
                                               entityViewEnginesDB);
        void RemoveEntityViewsFromEngines(
            Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEngines);
        void AddCapacity(int size);
        bool Remove(long idGid);
        ITypeSafeDictionary Create();
        int Count { get; }
        void FillWithIndexedEntityViews(ITypeSafeDictionary entityViews);
        void AddEntityViewsToEngines(FasterList<IHandleEntityViewEngineAbstracted> enginesForEntityView);
    }

    class TypeSafeDictionary<TValue> : FasterDictionary<long, TValue>, ITypeSafeDictionary where TValue : IEntityData
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

                    Add(entityView.ID.GID, entityView);
                }
            }
            catch (Exception e)
            {
                throw new TypeSafeDictionaryException(e);
            }
        }

        public void AddEntityViewsToEngines(FasterList<IHandleEntityViewEngineAbstracted> enginesForEntityView)
        {
            throw new NotImplementedException();
        }

        public void RemoveEntityFromDicAndEngines(EGID entityGid,
                                                  Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>>
                                                      entityViewEnginesDB)
        {
            
            
            TValue entity = this[entityGid.GID];

            RemoveEntityViewsFromEngines(entityViewEnginesDB, ref entity);

            Remove(entityGid.GID);
        }

        public void RemoveEntityViewsFromEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB, ref TValue entity)
        {
            FasterList<IHandleEntityViewEngineAbstracted> entityViewsEngines;
            if (entityViewEnginesDB.TryGetValue(typeof(TValue), out entityViewsEngines))
                for (int i = 0; i < entityViewEnginesDB.Count; i++)
                    (entityViewsEngines[i] as IHandleEntityStructEngine<TValue>).Remove(ref entity);
        }
        
        public void RemoveEntityViewsFromEngines(Dictionary<Type, FasterList<IHandleEntityViewEngineAbstracted>> entityViewEnginesDB)
        {
            int count;
            TValue[] values = this.GetFasterValuesBuffer(out count);

            for (int i = 0; i < count; i++)
            {
                TValue entity = values[i];

                RemoveEntityViewsFromEngines(entityViewEnginesDB, ref entity);
            }
        }

        public void AddCapacity(int size)
        {
            throw new NotImplementedException();
        }

        public ITypeSafeDictionary Create()
        {
            return new TypeSafeDictionary<TValue>();
        }
    }
}