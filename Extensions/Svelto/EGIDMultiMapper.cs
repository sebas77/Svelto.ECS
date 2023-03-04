using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    namespace Native
    {
        public struct EGIDMultiMapper<T> where T : unmanaged, _IInternalEntityComponent
        {
            public EGIDMultiMapper
            (SveltoDictionary<ExclusiveGroupStruct,
                 SveltoDictionary<uint, T, NativeStrategy<
                     SveltoDictionaryNode<uint>>, NativeStrategy<T>, NativeStrategy<int>>, 
                 ManagedStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>,
                 ManagedStrategy<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>
                   , NativeStrategy<int>>>, NativeStrategy<int>> dictionary)
            {
                _dic = dictionary;
            }

            public int count
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _dic.count;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ref T Entity(EGID entity)
            {
#if DEBUG && !PROFILE_SVELTO
                if (Exists(entity) == false)
                    throw new Exception("NativeEGIDMultiMapper: Entity not found");
#endif
                ref var sveltoDictionary = ref _dic.GetValueByRef(entity.groupID);
                return ref sveltoDictionary.GetValueByRef(entity.entityID);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Exists(EGID entity)
            {
                return _dic.TryFindIndex(entity.groupID, out var index)
                    && _dic.GetDirectValueByRef(index).ContainsKey(entity.entityID);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetEntity(EGID entity, out T component)
            {
                component = default;
                return _dic.TryFindIndex(entity.groupID, out var index)
                    && _dic.GetDirectValueByRef(index).TryGetValue(entity.entityID, out component);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool FindIndex(ExclusiveGroupStruct group, uint entityID, out uint index)
            {
                index = 0;
                return _dic.TryFindIndex(group, out var groupIndex) &&
                       _dic.GetDirectValueByRef(groupIndex).TryFindIndex(entityID, out index);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public uint GetIndex(ExclusiveGroupStruct group, uint entityID)
            {
                uint groupIndex = _dic.GetIndex(group);
                return _dic.GetDirectValueByRef(groupIndex).GetIndex(entityID);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool Exists(ExclusiveGroupStruct group, uint entityID)
            {
                return _dic.TryFindIndex(group, out var groupIndex) &&
                       _dic.GetDirectValueByRef(groupIndex).ContainsKey(entityID);
            }

			public Type entityType => TypeCache<T>.type;

            SveltoDictionary<ExclusiveGroupStruct,
                SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>,
                    NativeStrategy<int>>, ManagedStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>,
                ManagedStrategy<SveltoDictionary<uint, T, NativeStrategy<SveltoDictionaryNode<uint>>, NativeStrategy<T>,
                    NativeStrategy<int>>>, NativeStrategy<int>> _dic;
        }

        public interface IEGIDMultiMapper
        {
            bool FindIndex(ExclusiveGroupStruct group, uint entityID, out uint index);

            uint GetIndex(ExclusiveGroupStruct group, uint entityID);

            bool Exists(ExclusiveGroupStruct group, uint entityID);

			Type entityType { get; }
        }
    }

    public struct EGIDMultiMapper<T> where T : struct, IEntityViewComponent
    {
        public EGIDMultiMapper
        (SveltoDictionary<ExclusiveGroupStruct,
             SveltoDictionary<uint, T, ManagedStrategy<SveltoDictionaryNode<uint>>, ManagedStrategy<T>,
                 ManagedStrategy<int>>, ManagedStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>,
             ManagedStrategy<SveltoDictionary<uint, T, ManagedStrategy<SveltoDictionaryNode<uint>>, ManagedStrategy<T>,
                 ManagedStrategy<int>>>, ManagedStrategy<int>> dictionary)
        {
            _dic = dictionary;
        }

        public int count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dic.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(EGID entity)
        {
#if DEBUG && !PROFILE_SVELTO
            if (Exists(entity) == false)
                throw new Exception("NativeEGIDMultiMapper: Entity not found");
#endif
            ref var sveltoDictionary = ref _dic.GetValueByRef(entity.groupID);
            return ref sveltoDictionary.GetValueByRef(entity.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists(EGID entity)
        {
            return _dic.TryFindIndex(entity.groupID, out var index)
                && _dic.GetDirectValueByRef(index).ContainsKey(entity.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetEntity(EGID entity, out T component)
        {
            component = default;
            return _dic.TryFindIndex(entity.groupID, out var index)
                && _dic.GetDirectValueByRef(index).TryGetValue(entity.entityID, out component);
        }

        SveltoDictionary<ExclusiveGroupStruct,
            SveltoDictionary<uint, T, ManagedStrategy<SveltoDictionaryNode<uint>>, ManagedStrategy<T>,
                ManagedStrategy<int>>, ManagedStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>,
            ManagedStrategy<SveltoDictionary<uint, T, ManagedStrategy<SveltoDictionaryNode<uint>>, ManagedStrategy<T>,
                ManagedStrategy<int>>>, ManagedStrategy<int>> _dic;
    }
}