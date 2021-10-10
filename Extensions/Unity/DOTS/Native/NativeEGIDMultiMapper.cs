#if UNITY_NATIVE
using System;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;

namespace Svelto.ECS.Native
{
    /// <summary>
    /// Note: this class should really be ref struct by design. It holds the reference of a dictionary that can become
    /// invalid. Unfortunately it can be a ref struct, because Jobs needs to hold if by paramater. So the deal is
    /// that a job can use it as long as nothing else is modifying the entities database and the NativeEGIDMultiMapper
    /// is disposed right after the use.
    /// </summary>
    public struct NativeEGIDMultiMapper<T> : IDisposable where T : unmanaged, IEntityComponent
    {
        public NativeEGIDMultiMapper
        (SveltoDictionary<ExclusiveGroupStruct,
             SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>,
                 NativeStrategy<int>>, NativeStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>,
             NativeStrategy<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>,
                 NativeStrategy<int>>>, NativeStrategy<int>> dictionary)
        {
            _dic = dictionary;
        }

        public int count => (int) _dic.count;

        public void Dispose()
        {
            _dic.Dispose();
        }

        public ref T Entity(EGID entity)
        {
#if DEBUG && !PROFILE_SVELTO
            if (Exists(entity) == false)
                throw new Exception("NativeEGIDMultiMapper: Entity not found");
#endif
            ref var sveltoDictionary = ref _dic.GetValueByRef(entity.groupID);
            return ref sveltoDictionary.GetValueByRef(entity.entityID);
        }

        public bool Exists(EGID entity)
        {
            return _dic.TryFindIndex(entity.groupID, out var index)
                && _dic.GetDirectValueByRef(index).ContainsKey(entity.entityID);
        }

        public bool TryGetEntity(EGID entity, out T component)
        {
            component = default;
            return _dic.TryFindIndex(entity.groupID, out var index)
                && _dic.GetDirectValueByRef(index).TryGetValue(entity.entityID, out component);
        }
        
        SveltoDictionary<ExclusiveGroupStruct,
            SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>,
                NativeStrategy<int>>, NativeStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>,
            NativeStrategy<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>,
                NativeStrategy<int>>>, NativeStrategy<int>> _dic;
    }
}
#endif