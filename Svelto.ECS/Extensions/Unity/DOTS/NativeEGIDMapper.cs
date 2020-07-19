#if UNITY_BURST
using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public readonly struct NativeEGIDMapper<T> where T : unmanaged, IEntityComponent
    {
        readonly SveltoDictionaryNative<uint, T> map;
        public   ExclusiveGroupStruct      groupID { get; }

        public NativeEGIDMapper
        (ExclusiveGroupStruct groupStructId
       , SveltoDictionary<uint, T, NativeStrategy<FasterDictionaryNode<uint>>, NativeStrategy<T>> toNative) : this()
        {
            groupID = groupStructId;
            map     = toNative;
        }

        public uint Count => map.count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
#if DEBUG && !PROFILE_SVELTO
            if (map.TryFindIndex(entityID, out var findIndex) == false)
                throw new Exception("Entity not found in this group ".FastConcat(typeof(T).ToString()));
#else
            map.TryFindIndex(entityID, out var findIndex);
#endif
            return ref map.GetDirectValueByRef(findIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEntity(uint entityID, out T value)
        {
            if (map.count > 0 && map.TryFindIndex(entityID, out var index))
            {
                unsafe
                {
                    value = Unsafe.AsRef<T>(Unsafe.Add<T>((void*) map.GetValues(out _).ToNativeArray(out _)
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
            if (map.TryFindIndex(entityID, out index))
            {
                return new NB<T>((IntPtr) map.GetValues(out var count).ToNativeArray(out _), count);
            }

            throw new ECSException("Entity not found");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetArrayAndEntityIndex(uint entityID, out uint index, out NB<T> array)
        {
            index = 0;
            if (map.count > 0 && map.TryFindIndex(entityID, out index))
            {
                array = new NB<T>((IntPtr) map.GetValues(out var count).ToNativeArray(out _), count);
                return true;
            }

            array = default;
            return false;
        }

        public bool Exists(uint idEntityId)
        {
            return map.count > 0 && map.TryFindIndex(idEntityId, out _);
        }
    }
}
#endif