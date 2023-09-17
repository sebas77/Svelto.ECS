using System;
using Svelto.Common;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Native
{
    /// <summary>
    /// Note: this class should really be ref struct by design. It holds the reference of a dictionary that can become
    /// invalid. Unfortunately it can be a ref struct, because Jobs needs to hold if by paramater. So the deal is
    /// that a job can use it as long as nothing else is modifying the entities database and the NativeEGIDMultiMapper
    /// is disposed right after the use.
    ///
    ///WARNING: REMEMBER THIS MUST BE DISPOSED OF, AS IT USES NATIVE MEMORY. IT WILL LEAK MEMORY OTHERWISE
    ///
    /// to retrieve a NativeEGIDMultiMapper use entitiesDB.QueryNativeMappedEntities<T>(groups, Svelto.Common.Allocator.TempJob);
    ///
    /// TODO: this could be extended to support all the query interfaces so that NB can become ref and this used to query entities inside jobs
    /// 
    /// </summary>
    public struct NativeEGIDMultiMapper<T> : IEGIDMultiMapper, IDisposable where T : unmanaged, _IInternalEntityComponent
    {
        internal NativeEGIDMultiMapper(in SveltoDictionaryNative<ExclusiveGroupStruct, SharedSveltoDictionaryNative<uint, T>> dictionary)
        {
            _dic = dictionary;
        }

        public int count => (int)_dic.count;

        public void Dispose()
        {
            _dic.Dispose();
        }

        public ref T Entity(EGID entity)
        {
#if DEBUG && !PROFILE_SVELTO
            if (Exists(entity) == false)
                throw new Exception($"NativeEGIDMultiMapper: Entity not found {entity}");
#endif
            ref var sveltoDictionary = ref _dic.GetValueByRef(entity.groupID);
            return ref sveltoDictionary.dictionary.GetValueByRef(entity.entityID);
        }

        public uint GetIndex(EGID entity)
        {
#if DEBUG && !PROFILE_SVELTO
            if (Exists(entity) == false)
                throw new Exception($"NativeEGIDMultiMapper: Entity not found {entity}");
#endif
            ref var sveltoDictionary = ref _dic.GetValueByRef(entity.groupID);
            return sveltoDictionary.dictionary.GetIndex(entity.entityID);
        }

        public bool Exists(EGID entity)
        {
            return _dic.TryFindIndex(entity.groupID, out var index) &&
                _dic.GetDirectValueByRef(index).dictionary.ContainsKey(entity.entityID);
        }

        public bool TryGetEntity(EGID entity, out T component)
        {
            component = default;
            return _dic.TryFindIndex(entity.groupID, out var index) &&
                _dic.GetDirectValueByRef(index).dictionary.TryGetValue(entity.entityID, out component);
        }

        SveltoDictionaryNative<ExclusiveGroupStruct, SharedSveltoDictionaryNative<uint, T>> _dic;

        public Type entityType => TypeCache<T>.type;
    }
}