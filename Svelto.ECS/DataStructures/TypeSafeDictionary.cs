using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    sealed class TypeSafeDictionary<TValue> : ITypeSafeDictionary<TValue> where TValue : struct, IEntityComponent
    {
        static readonly Type   _type     = typeof(TValue);
        static readonly string _typeName = _type.Name;
        static readonly bool   _hasEgid  = typeof(INeedEGID).IsAssignableFrom(_type);

        public TypeSafeDictionary(uint size)
        {
            _implementation = new FasterDictionary<uint, TValue>(size);
        }

        public TypeSafeDictionary()
        {
            _implementation = new FasterDictionary<uint, TValue>(1);
        }

        /// <summary>
        /// Add entities from external typeSafeDictionary
        /// </summary>
        /// <param name="entitiesToSubmit"></param>
        /// <param name="groupId"></param>
        /// <exception cref="TypeSafeDictionaryException"></exception>
        public void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId) 
        {
            var typeSafeDictionary = entitiesToSubmit as TypeSafeDictionary<TValue>;

            foreach (var tuple in typeSafeDictionary)
            {
                try
                {
                    if (_hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref tuple.Value, new EGID(tuple.Key, groupId));

                    _implementation.Add(tuple.Key, tuple.Value);
                }
                catch (Exception e)
                {
                    throw new
                        TypeSafeDictionaryException("trying to add an EntityComponent with the same ID more than once Entity: ".FastConcat(typeof(TValue).ToString()).FastConcat(", group ").FastConcat(groupId).FastConcat(", id ").FastConcat(tuple.Key),
                                                    e);
                }
            }
        }
        
        public void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup)
        {
            var valueIndex = _implementation.GetIndex(fromEntityGid.entityID);

            if (toGroup != null)
            {
                var     toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                ref var entity        = ref _implementation.unsafeValues[(int) valueIndex];

                if (_hasEgid) SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID);

                toGroupCasted.Add(fromEntityGid.entityID, entity);
            }
        }

        public void AddEntitiesToEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityComponentEnginesDB,
                                         ITypeSafeDictionary                                     realDic,
                                         ExclusiveGroupStruct                     @group,
                                         in PlatformProfiler profiler)
        {
            var typeSafeDictionary = realDic as ITypeSafeDictionary<TValue>;

            //this can be optimized, should pass all the entities and not restart the process for each one
            foreach (var value in _implementation)
                AddEntityComponentToEngines(entityComponentEnginesDB, ref typeSafeDictionary.GetValueByRef(value.Key), null,
                                       in profiler, new EGID(value.Key, group));
        }

        public void RemoveEntitiesFromEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityComponentEnginesDB, in PlatformProfiler profiler,
            ExclusiveGroupStruct                     @group)
        {
            foreach (var value in _implementation)
                RemoveEntityComponentFromEngines(entityComponentEnginesDB, ref _implementation.GetValueByRef(value.Key), null,
                                            in profiler, new EGID(value.Key, group));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear() { _implementation.FastClear(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Has(uint key) { return _implementation.ContainsKey(key); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntityFromDictionary(EGID fromEntityGid)
        {
            _implementation.Remove(fromEntityGid.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(uint size) { _implementation.SetCapacity(size); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim() { _implementation.Trim(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() { _implementation.Clear(); }

        public void MoveEntityFromEngines(EGID                                                    fromEntityGid,
                                          EGID?                                                   toEntityID, ITypeSafeDictionary toGroup,
                                          FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines,
                                          in PlatformProfiler                                     profiler)
        {
            var valueIndex = _implementation.GetIndex(fromEntityGid.entityID);

            ref var entity = ref _implementation.unsafeValues[(int) valueIndex];

            if (toGroup != null)
            {
                RemoveEntityComponentFromEngines(engines, ref entity, fromEntityGid.groupID, in profiler, fromEntityGid);

                var toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                var previousGroup = fromEntityGid.groupID;

                if (_hasEgid) SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID.Value);

                var index = toGroupCasted.GetIndex(toEntityID.Value.entityID);

                AddEntityComponentToEngines(engines, ref toGroupCasted.unsafeValues[(int) index], previousGroup, in profiler,
                                       toEntityID.Value);
            }
            else
                RemoveEntityComponentFromEngines(engines, ref entity, null, in profiler, fromEntityGid);
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _implementation.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSafeDictionary Create() { return new TypeSafeDictionary<TValue>(); }

        void AddEntityComponentToEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityComponentEnginesDB,
                                    ref TValue                                              entity,
                                    ExclusiveGroupStruct?                    previousGroup,
                                    in PlatformProfiler                                     profiler,
                                    EGID                                                    egid)
        {
            //get all the engines linked to TValue
            if (!entityComponentEnginesDB.TryGetValue(new RefWrapper<Type>(_type), out var entityComponentsEngines)) return;

            if (previousGroup == null)
            {
                for (var i = 0; i < entityComponentsEngines.count; i++)
                    try
                    {
                        using (profiler.Sample(entityComponentsEngines[i], _typeName))
                        {
                            (entityComponentsEngines[i] as IReactOnAddAndRemove<TValue>).Add(ref entity, egid);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new
                            ECSException("Code crashed inside Add callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
            else
            {
                for (var i = 0; i < entityComponentsEngines.count; i++)
                    try
                    {
                        using (profiler.Sample(entityComponentsEngines[i], _typeName))
                        {
                            (entityComponentsEngines[i] as IReactOnSwap<TValue>).MovedTo(ref entity, previousGroup.Value,
                                                                                    egid);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new
                            ECSException("Code crashed inside MovedTo callback ".FastConcat(typeof(TValue).ToString()),
                                         e);
                    }
            }
        }

        static void RemoveEntityComponentFromEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> @group,
                                                ref TValue                                              entity,
                                                uint?                    previousGroup,
                                                in PlatformProfiler                                     profiler,
                                                EGID                                                    egid)
        {
            if (!@group.TryGetValue(new RefWrapper<Type>(_type), out var entityComponentsEngines)) return;

            if (previousGroup == null)
            {
                for (var i = 0; i < entityComponentsEngines.count; i++)
                    try
                    {
                        using (profiler.Sample(entityComponentsEngines[i], _typeName))
                            (entityComponentsEngines[i] as IReactOnAddAndRemove<TValue>).Remove(ref entity, egid);
                    }
                    catch (Exception e)
                    {
                        throw new
                            ECSException("Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()),
                                         e);
                    }
            }
#if SEEMS_UNNECESSARY
            else
            {
                for (var i = 0; i < entityComponentsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityComponentsEngines[i], _typeName))
                            (entityComponentsEngines[i] as IReactOnSwap<TValue>).MovedFrom(ref entity, egid);
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue[] GetValuesArray(out uint count)
        {
            var managedBuffer = _implementation.GetValuesArray(out count);
            return managedBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(uint egidEntityId) { return _implementation.ContainsKey(egidEntityId); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint egidEntityId, in TValue entityComponent) { _implementation.Add(egidEntityId, entityComponent); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterDictionary<uint, TValue>.FasterDictionaryKeyValueEnumerator GetEnumerator()
        {
            return _implementation.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(uint key) { return ref _implementation.GetValueByRef(key); }

        public ref TValue this[uint idEntityId]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _implementation.GetValueByRef(idEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint valueEntityId) { return _implementation.GetIndex(valueEntityId); }

        public TValue[] unsafeValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _implementation.unsafeValues;
        }

        public object GenerateSentinel()
        {
            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(uint entityId, out TValue item) { return _implementation.TryGetValue(entityId, out item); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(uint idEntityId) { return ref _implementation.GetOrCreate(idEntityId); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(uint entityId, out uint index) { return _implementation.TryFindIndex(entityId, out index); }

        internal FasterDictionary<uint, TValue> implementation => _implementation;
        
        readonly FasterDictionary<uint, TValue> _implementation;
    }
}