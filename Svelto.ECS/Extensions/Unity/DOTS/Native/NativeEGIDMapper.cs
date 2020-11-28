#if UNITY_NATIVE
using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public readonly struct NativeEGIDMapper<T>:IEGIDMapper where T : unmanaged, IEntityComponent
    {
        public NativeEGIDMapper
        (ExclusiveGroupStruct groupStructId, SveltoDictionaryNative<uint, T> toNative) : this()
        {
            groupID = groupStructId;
            _map     = toNative;
        }

        public int   count => _map.count;
        public Type entityType  => TypeCache<T>.type;
        public   ExclusiveGroupStruct                    groupID { get; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
#if DEBUG && !PROFILE_SVELTO
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
            {
                unsafe
                {
                    value = Unsafe.AsRef<T>(Unsafe.Add<T>((void*) _map.GetValues(out _).ToNativeArray(out _)
                                                        , (int) index));
                    return true;
                }
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NB<T> GetArrayAndEntityIndex(uint entityID, out uint index)
        {
            if (_map.TryFindIndex(entityID, out index))
            {
                return new NB<T>(_map.GetValues(out var count).ToNativeArray(out _), count);
            }

            throw new ECSException("Entity not found");
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
        
        readonly ReadonlySveltoDictionaryNative<uint, T> _map;
    }
}
#endif