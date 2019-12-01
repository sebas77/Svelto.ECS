using System;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary
    {
        int Count { get; }
        ITypeSafeDictionary Create();

        void AddEntitiesToEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDb,
            ITypeSafeDictionary realDic, in PlatformProfiler profiler, ExclusiveGroup.ExclusiveGroupStruct @group);

        void RemoveEntitiesFromEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
            in PlatformProfiler profiler, ExclusiveGroup.ExclusiveGroupStruct @group);

        void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId);

        void MoveEntityFromEngines(EGID fromEntityGid, EGID? toEntityID, ITypeSafeDictionary toGroup,
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines, in PlatformProfiler profiler);

        void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup);
        void RemoveEntityFromDictionary(EGID fromEntityGid, in PlatformProfiler profiler);

        void SetCapacity(uint size);
        void Trim();
        void Clear();
        bool Has(uint entityIdEntityId);
    }

    class TypeSafeDictionary<TValue> : FasterDictionary<uint, TValue>,
        ITypeSafeDictionary where TValue : struct, IEntityStruct
    {
        static readonly Type   _type     = typeof(TValue);
        static readonly string _typeName = _type.Name;
        static readonly bool   _hasEgid  = typeof(INeedEGID).IsAssignableFrom(_type);

        public TypeSafeDictionary(uint size) : base(size) {}
        public TypeSafeDictionary() {}

        public void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId)
        {
            var typeSafeDictionary = entitiesToSubmit as TypeSafeDictionary<TValue>;

            foreach (var tuple in typeSafeDictionary)
            {
                try
                {
                    if (_hasEgid) SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref tuple.Value, new EGID(tuple.Key, groupId));

                    Add(tuple.Key, tuple.Value);
                }
                catch (Exception e)
                {
                    throw new TypeSafeDictionaryException(
                        "trying to add an EntityView with the same ID more than once Entity: "
                            .FastConcat(typeof(TValue).ToString()).FastConcat(", group ").FastConcat(groupId).FastConcat(", id ").FastConcat(tuple.Key), e);
                }
            }
        }

        public void AddEntitiesToEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
            ITypeSafeDictionary realDic, in PlatformProfiler profiler, ExclusiveGroup.ExclusiveGroupStruct @group)
        {
            var typeSafeDictionary = realDic as TypeSafeDictionary<TValue>;

            //this can be optimized, should pass all the entities and not restart the process for each one
            foreach (var value in this)
                AddEntityViewToEngines(entityViewEnginesDB, ref typeSafeDictionary.GetValueByRef(value.Key), null,
                    in profiler, new EGID(value.Key, group));
        }

        public void RemoveEntitiesFromEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
            in PlatformProfiler profiler, ExclusiveGroup.ExclusiveGroupStruct @group)
        {
            foreach (var value in this)
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref GetValueByRef(value.Key), null, in profiler,
                    new EGID(value.Key, group));
        }

        public bool Has(uint entityIdEntityId)
        {
            return ContainsKey(entityIdEntityId);
        }

        public void RemoveEntityFromDictionary(EGID fromEntityGid, in PlatformProfiler profiler)
        {
            Remove(fromEntityGid.entityID);
        }

        public void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup)
        {
            var valueIndex = GetIndex(fromEntityGid.entityID);

            if (toGroup != null)
            {
                var toGroupCasted = toGroup as TypeSafeDictionary<TValue>;
                ref var entity = ref valuesArray[valueIndex];

                if (_hasEgid) SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID);

                toGroupCasted.Add(fromEntityGid.entityID, entity);
            }
        }

        public void MoveEntityFromEngines(EGID fromEntityGid, EGID? toEntityID, ITypeSafeDictionary toGroup,
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines, in PlatformProfiler profiler)
        {
            var valueIndex = GetIndex(fromEntityGid.entityID);
            
            ref var entity = ref valuesArray[valueIndex];

            if (toGroup != null)
            {
                RemoveEntityViewFromEngines(engines, ref entity, fromEntityGid.groupID, in profiler,
                    fromEntityGid);

                var toGroupCasted = toGroup as TypeSafeDictionary<TValue>;
                var previousGroup = fromEntityGid.groupID;

                if (_hasEgid) SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID.Value);

                var index = toGroupCasted.GetIndex(toEntityID.Value.entityID);

                AddEntityViewToEngines(engines, ref toGroupCasted.valuesArray[index], previousGroup,
                    in profiler, toEntityID.Value);
            }
            else
                RemoveEntityViewFromEngines(engines, ref entity, null, in profiler, fromEntityGid);
        }

        public ITypeSafeDictionary Create()
        {
            return new TypeSafeDictionary<TValue>();
        }

        void AddEntityViewToEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
            ref TValue entity, ExclusiveGroup.ExclusiveGroupStruct? previousGroup,
            in PlatformProfiler profiler, EGID egid)
        {
            //get all the engines linked to TValue
            if (!entityViewEnginesDB.TryGetValue(new RefWrapper<Type>(_type), out var entityViewsEngines)) return;

            if (previousGroup == null)
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                        {
                            (entityViewsEngines[i] as IReactOnAddAndRemove<TValue>).Add(ref entity, egid);
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
                            (entityViewsEngines[i] as IReactOnSwap<TValue>).MovedTo(ref entity, previousGroup.Value,
                                egid);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside MovedTo callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
        }

        static void RemoveEntityViewFromEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> @group, ref TValue entity,
            ExclusiveGroup.ExclusiveGroupStruct? previousGroup, in PlatformProfiler profiler, EGID egid)
        {
            if (!@group.TryGetValue(new RefWrapper<Type>(_type), out var entityViewsEngines)) return;

            if (previousGroup == null)
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                            (entityViewsEngines[i] as IReactOnAddAndRemove<TValue>).Remove(ref entity, egid);
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
#if SEEMS_UNNECESSARY            
            else
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                            (entityViewsEngines[i] as IReactOnSwap<TValue>).MovedFrom(ref entity, egid);
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
#endif
        }
    }
}