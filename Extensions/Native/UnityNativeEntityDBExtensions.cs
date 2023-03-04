using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Native
{
    public static class UnityNativeEntityDBExtensions
    {
        static NativeEGIDMapper<T> ToNativeEGIDMapper<T>(this UnmanagedTypeSafeDictionary<T> dic,
            ExclusiveGroupStruct groupStructId) where T : unmanaged, IEntityComponent
        {
            var mapper = new NativeEGIDMapper<T>(groupStructId, dic.implUnmgd);

            return mapper;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NativeEGIDMapper<T>  QueryNativeMappedEntities<T>(this EntitiesDB entitiesDb, ExclusiveGroupStruct groupStructId)
            where T : unmanaged, IEntityComponent
        {
            if (entitiesDb.SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(typeof(T), groupStructId.ToName());

            return (typeSafeDictionary as UnmanagedTypeSafeDictionary<T>).ToNativeEGIDMapper(groupStructId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryQueryNativeMappedEntities<T>(this EntitiesDB entitiesDb, ExclusiveGroupStruct groupStructId,
                                                           out NativeEGIDMapper<T> mapper)
            where T : unmanaged, IEntityComponent
        {
            mapper = NativeEGIDMapper<T>.empty;
            if (entitiesDb.SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false ||
                typeSafeDictionary.count == 0)
                return false;

            mapper = (typeSafeDictionary as UnmanagedTypeSafeDictionary<T>).ToNativeEGIDMapper(groupStructId);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        ///Note: if I use a SharedNativeSveltoDictionary for implUnmg, I may be able to cache NativeEGIDMultiMapper
        /// and reuse it
        /// TODO: the ability to retain a NativeEGIDMultiMapper thanks to the use of shareable arrays
        /// must be unit tested!
        public static NativeEGIDMultiMapper<T> QueryNativeMappedEntities<T>(this EntitiesDB entitiesDb,
                    LocalFasterReadOnlyList<ExclusiveGroupStruct> groups, Allocator allocator)
            where T : unmanaged, _IInternalEntityComponent
        {
            var dictionary = new SveltoDictionaryNative<ExclusiveGroupStruct, SharedSveltoDictionaryNative<uint, T>>  
                    ((uint) groups.count, allocator);
        
            foreach (var group in groups)
            {
                if (entitiesDb.SafeQueryEntityDictionary<T>(group, out var typeSafeDictionary) == true)
                    //if (typeSafeDictionary.count > 0)
                        dictionary.Add(group, ((UnmanagedTypeSafeDictionary<T>)typeSafeDictionary).implUnmgd);
            }
            
            return new NativeEGIDMultiMapper<T>(dictionary);
        }
    }
}
