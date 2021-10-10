#if UNITY_NATIVE
using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;

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
        public NativeEGIDMapper
        (ExclusiveGroupStruct groupStructId
       , SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>, NativeStrategy<int>>
             toNative) : this()
        {
            groupID = groupStructId;
            _map    = new SveltoDictionaryNative<uint, T>();
            
            _map.UnsafeCast(toNative);
        }

        public int                  count      => _map.count;
        public Type                 entityType => TypeCache<T>.type;
        public ExclusiveGroupStruct groupID    { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
#if DEBUG
            if (_map.TryFindIndex(entityID, out var findIndex) == false)
                throw new Exception("Entity not found in this group ".FastConcat(typeof(T).ToString()));
#else
            _map.TryFindIndex(entityID, out var findIndex);
#endif
            return ref _map.GetDirectValueByRef(findIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEntity(uint entityID, out T value)
        {
            if (_map.count > 0 && _map.TryFindIndex(entityID, out var index))
                unsafe
                {
                    value = Unsafe.AsRef<T>(Unsafe.Add<T>((void*) _map.GetValues(out _).ToNativeArray(out _)
                                                        , (int) index));
                    return true;
                }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NB<T> GetArrayAndEntityIndex(uint entityID, out uint index)
        {
            if (_map.TryFindIndex(entityID, out index))
                return new NB<T>(_map.GetValues(out var count).ToNativeArray(out _), count);

#if DEBUG
            throw new ECSException("Entity not found");
#else
            return default;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetArrayAndEntityIndex(uint entityID, out uint index, out NB<T> array)
        {
            index = 0;
            if (_map.count > 0 && _map.TryFindIndex(entityID, out index))
            {
                array = new NB<T>(_map.GetValues(out var count).ToNativeArray(out _), count);
                return true;
            }

            array = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(uint idEntityId)
        {
            return _map.count > 0 && _map.TryFindIndex(idEntityId, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint entityID)
        {
            return _map.GetIndex(entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FindIndex(uint valueKey, out uint index)
        {
            return _map.TryFindIndex(valueKey, out index);
        }

        readonly SveltoDictionaryNative<uint, T> _map;
    }
}
#endif