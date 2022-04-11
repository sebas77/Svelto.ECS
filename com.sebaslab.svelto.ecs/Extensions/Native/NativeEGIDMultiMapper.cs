using System;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;

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
    /// </summary>
    public struct NativeEGIDMultiMapper<T> : IDisposable where T : unmanaged, IEntityComponent
    {
        public NativeEGIDMultiMapper(in SveltoDictionary<
            /*key  */ExclusiveGroupStruct,
            /*value*/
            SharedNative<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>,
                NativeStrategy<int>>>,
            /*strategy to store the key*/ NativeStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>,
            /*strategy to store the value*/
            NativeStrategy<SharedNative<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>,
                NativeStrategy<T>, NativeStrategy<int>>>>, NativeStrategy<int>> dictionary)
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
            return ref sveltoDictionary.value.GetValueByRef(entity.entityID);
        }

        public uint GetIndex(EGID entity)
        {
#if DEBUG && !PROFILE_SVELTO
            if (Exists(entity) == false)
                throw new Exception($"NativeEGIDMultiMapper: Entity not found {entity}");
#endif
            ref var sveltoDictionary = ref _dic.GetValueByRef(entity.groupID);
            return sveltoDictionary.value.GetIndex(entity.entityID);
        }

        public bool Exists(EGID entity)
        {
            return _dic.TryFindIndex(entity.groupID, out var index) &&
                _dic.GetDirectValueByRef(index).value.ContainsKey(entity.entityID);
        }

        public bool TryGetEntity(EGID entity, out T component)
        {
            component = default;
            return _dic.TryFindIndex(entity.groupID, out var index) &&
                _dic.GetDirectValueByRef(index).value.TryGetValue(entity.entityID, out component);
        }

        SveltoDictionary<ExclusiveGroupStruct,
            SharedNative<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>,
                NativeStrategy<int>>>, NativeStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>, NativeStrategy<
                SharedNative<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>,
                    NativeStrategy<int>>>>, NativeStrategy<int>> _dic;
    }
}