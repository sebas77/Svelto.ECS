using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS.Native
{
    /// <summary>
    /// Note: this class should really be ref struct by design. It holds the reference of a dictionary that can become
    /// invalid. Unfortunately it can be a ref struct, because Jobs needs to hold if by paramater. So the deal is
    /// that a job can use it as long as nothing else is modifying the entities database and the NativeEGIDMapper
    /// is disposed right after the use.
    /// </summary>
    public readonly struct NativeEGIDMapper<T> : IEGIDMapper where T : unmanaged, IEntityComponent
    {
        public static readonly NativeEGIDMapper<T> empty = new NativeEGIDMapper<T>
            (default, new SharedNative<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>,
                NativeStrategy<T>, NativeStrategy<int>>>(
                new SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>,
                    NativeStrategy<T>, NativeStrategy<int>>(0, Allocator.Persistent)));
        
        public NativeEGIDMapper(ExclusiveGroupStruct groupStructId,
            in SharedNative<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>,
                NativeStrategy<int>>> toNative) : this()
        {
            groupID = groupStructId;
            _map    = toNative;
        }

        public int                  count      => _map.value.count;
        public Type                 entityType => TypeCache<T>.type;
        public ExclusiveGroupStruct groupID    { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
            var sveltoDictionary = _map.value;
                    
#if DEBUG && !PROFILE_SVELTO
            if (sveltoDictionary.TryFindIndex(entityID, out var findIndex) == false)
                throw new Exception($"Entity {entityID} not found in this group {groupID} - {typeof(T).Name}");
#else
            sveltoDictionary.TryFindIndex(entityID, out var findIndex);
#endif
            return ref sveltoDictionary.GetDirectValueByRef(findIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEntity(uint entityID, out T value)
        {
            var sveltoDictionary = _map.value;
            if (sveltoDictionary.count > 0 && sveltoDictionary.TryFindIndex(entityID, out var index))
            {
                var values = sveltoDictionary.unsafeValues.ToRealBuffer();
                value = values[index];
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NB<T> GetArrayAndEntityIndex(uint entityID, out uint index)
        {
            var sveltoDictionary = _map.value;
            if (sveltoDictionary.TryFindIndex(entityID, out index))
                return sveltoDictionary.unsafeValues.ToRealBuffer();

#if DEBUG && !PROFILE_SVELTO
            throw new ECSException("Entity not found");
#else
            return default;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetArrayAndEntityIndex(uint entityID, out uint index, out NB<T> array)
        {
            index = 0;
            var sveltoDictionary = _map.value;
            
            if (sveltoDictionary.count > 0 && sveltoDictionary.TryFindIndex(entityID, out index))
            {
                array = sveltoDictionary.unsafeValues.ToRealBuffer();
                return true;
            }

            array = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(uint idEntityId)
        {
            var sveltoDictionary = _map.value;
            return sveltoDictionary.count > 0 && sveltoDictionary.TryFindIndex(idEntityId, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint entityID)
        {
            return _map.value.GetIndex(entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FindIndex(uint valueKey, out uint index)
        {
            return _map.value.TryFindIndex(valueKey, out index);
        }

        readonly SharedNative<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>,
            NativeStrategy<int>>> _map;
    }
}