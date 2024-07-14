using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    //this cannot be inside EntitiesDB otherwise it will cause hashing of reference in Burst
    public class Internal_FilterHelper
    {
        //since the user can choose their own filterID, in order to avoid collisions between
        //filters of the same type, the FilterContext is provided. The type is identified through
        //ComponentTypeID<T>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static CombinedFilterComponentID CombineFilterIDWithComponentID<T>(CombinedFilterID combinedFilterID)
                where T : struct, _IInternalEntityComponent
        {
            return combinedFilterID.CombineComponent<T>();
        }
    }

    public partial class EntitiesDB
    {
        public SveltoFilters GetFilters()
        {
            return new SveltoFilters(
                _enginesRoot._persistentEntityFilters,
                _enginesRoot._indicesOfPersistentFiltersUsedByThisComponent,
                _enginesRoot._transientEntityFilters,
                _enginesRoot._indicesOfTransientFiltersUsedByThisComponent);
        }

        /// <summary>
        /// this whole structure is usable inside DOTS JOBS and BURST
        /// </summary>
        public readonly struct SveltoFilters
        {
            static readonly SharedStaticWrapper<int, Internal_FilterHelper> uniqueContextID;

#if UNITY_BURST
            [Unity.Burst.BurstDiscard] 
            //SharedStatic values must be initialized from not burstified code
#endif
            public static FilterContextID GetNewContextID()
            {
                return new FilterContextID((ushort)Interlocked.Increment(ref uniqueContextID.Data));
            }

            internal SveltoFilters(SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection> persistentEntityFilters,
                SharedSveltoDictionaryNative<ComponentID, NativeDynamicArrayCast<int>> indicesOfPersistentFiltersUsedByThisComponent,
                SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection> transientEntityFilters,
                SharedSveltoDictionaryNative<ComponentID, NativeDynamicArrayCast<int>> indicesOfTransientFiltersUsedByThisComponent)
            {
                _persistentEntityFilters = persistentEntityFilters;
                _indicesOfPersistentFiltersUsedByThisComponent = indicesOfPersistentFiltersUsedByThisComponent;
                _transientEntityFilters = transientEntityFilters;
                _indicesOfTransientFiltersUsedByThisComponent = indicesOfTransientFiltersUsedByThisComponent;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollection GetOrCreatePersistentFilter<T>(int filterID, FilterContextID filterContextId)
                    where T : struct, _IInternalEntityComponent
            {
                return GetOrCreatePersistentFilter<T>(new CombinedFilterID(filterID, filterContextId));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollection GetOrCreatePersistentFilter<T>(CombinedFilterID filterID) where T : struct, _IInternalEntityComponent
            {
                var componentAndFilterID = Internal_FilterHelper.CombineFilterIDWithComponentID<T>(filterID);

                if (_persistentEntityFilters.TryFindIndex(componentAndFilterID, out var index) == true)
                    return _persistentEntityFilters.GetDirectValueByRef(index);

                _persistentEntityFilters.Add(componentAndFilterID, new EntityFilterCollection(filterID));

                var lastIndex = _persistentEntityFilters.count - 1;

                var componentId = ComponentTypeID<T>.id;
                if (_indicesOfPersistentFiltersUsedByThisComponent.TryFindIndex(componentId, out var getIndex) == false)
                {
                    var newArray = new NativeDynamicArrayCast<int>(1, Allocator.Persistent);
                    newArray.Add(lastIndex);
                    _indicesOfPersistentFiltersUsedByThisComponent.Add(componentId, newArray);
                }
                else
                {
                    ref var array = ref _indicesOfPersistentFiltersUsedByThisComponent.GetDirectValueByRef(getIndex);
                    array.Add(lastIndex);
                }

                return _persistentEntityFilters.GetDirectValueByRef((uint)lastIndex);
            }

            /// <summary>
            /// Create a persistent filter. Persistent filters are not deleted after each submission,
            /// however they have a maintenance cost that must be taken into account and will affect
            /// entities submission performance.
            /// Persistent filters keep track of the entities group swaps and they are automatically updated accordingly.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
#if UNITY_BURST && UNITY_COLLECTIONS
            [Unity.Burst.BurstDiscard] //not burst compatible because of  ComponentTypeID<T>.id and GetOrAdd callback;
#endif
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollection CreatePersistentFilter<T>(CombinedFilterID filterID)
                    where T : struct, _IInternalEntityComponent
            {
                var componentAndFilterID = Internal_FilterHelper.CombineFilterIDWithComponentID<T>(filterID);

                if (_persistentEntityFilters.TryFindIndex(componentAndFilterID, out var index) == true)
                    throw new ECSException("filter already exists");

                var filterCollection = new EntityFilterCollection(filterID);

                _persistentEntityFilters.Add(componentAndFilterID, filterCollection);

                var lastIndex = _persistentEntityFilters.count - 1;

                _indicesOfPersistentFiltersUsedByThisComponent.GetOrAdd(ComponentTypeID<T>.id, _builder).Add(lastIndex);

                return _persistentEntityFilters.GetDirectValueByRef((uint)lastIndex);
            }

            static NativeDynamicArrayCast<int> Builder()
            {
                return new NativeDynamicArrayCast<int>(1, Allocator.Persistent);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollection GetPersistentFilter<T>(int filterID, FilterContextID filterContextId)
                    where T : struct, _IInternalEntityComponent
            {
                return GetPersistentFilter<T>(new CombinedFilterID(filterID, filterContextId));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollection GetPersistentFilter<T>(CombinedFilterID filterID)
                    where T : struct, _IInternalEntityComponent
            {
                var componentAndFilterID = Internal_FilterHelper.CombineFilterIDWithComponentID<T>(filterID);

                if (_persistentEntityFilters.TryFindIndex(componentAndFilterID, out var index) == true)
                    return _persistentEntityFilters.GetDirectValueByRef(index);

                throw new ECSException("filter not found");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetPersistentFilter<T>(CombinedFilterID combinedFilterID,
                out EntityFilterCollection entityCollection)
                    where T : struct, _IInternalEntityComponent
            {
                var componentAndFilterID = Internal_FilterHelper.CombineFilterIDWithComponentID<T>(combinedFilterID);

                if (_persistentEntityFilters.TryFindIndex(componentAndFilterID, out var index) == true)
                {
                    entityCollection = _persistentEntityFilters.GetDirectValueByRef(index);
                    return true;
                }

                entityCollection = default;
                return false;
            }

            /// <summary>
            /// Svelto.ECS tracks the filters linked to each
            /// component. This allows to iterate over all the filters of a given filter context linked to a component.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollectionsEnumerator GetPersistentFilters<T>()
                    where T : struct, _IInternalEntityComponent
            {
                if (_indicesOfPersistentFiltersUsedByThisComponent.TryFindIndex(ComponentTypeID<T>.id, out var index) == true)
                    return new EntityFilterCollectionsEnumerator(
                        _indicesOfPersistentFiltersUsedByThisComponent.GetDirectValueByRef(index),
                        _persistentEntityFilters);

                throw new ECSException($"no filters associated with the type {TypeCache<T>.name}");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollectionsWithContextEnumerator GetPersistentFilters<T>(FilterContextID filterContextId)
                    where T : struct, _IInternalEntityComponent
            {
                if (_indicesOfPersistentFiltersUsedByThisComponent.TryFindIndex(ComponentTypeID<T>.id, out var index) == true)
                    return new EntityFilterCollectionsWithContextEnumerator(
                        _indicesOfPersistentFiltersUsedByThisComponent.GetDirectValueByRef(index),
                        _persistentEntityFilters, filterContextId);

                throw new ECSException($"no filters associated with the type {TypeCache<T>.name}");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetPersistentFilters<T>(FilterContextID filterContextId,
                out EntityFilterCollectionsWithContextEnumerator enumerator)
                    where T : struct, _IInternalEntityComponent
            {
                if (_indicesOfPersistentFiltersUsedByThisComponent.TryFindIndex(ComponentTypeID<T>.id, out var index) == true)
                {
                    ref var filterIndices = ref _indicesOfPersistentFiltersUsedByThisComponent.GetDirectValueByRef(index);
                    enumerator = new EntityFilterCollectionsWithContextEnumerator(filterIndices, _persistentEntityFilters, filterContextId);

                    return true;
                }

                enumerator = default;
                return false;
            }

            /// <summary>
            /// Creates a transient filter. Transient filters are deleted after each submission
            /// transient filters are identified by filterID and Context and can be linked to several groups.
            /// So for each group there can be as many as necessary transient filters with different ID and contextID
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollection GetOrCreateTransientFilter<T>(CombinedFilterID combinedFilterID, bool trackFilter = false)
                    where T : struct, _IInternalEntityComponent
            {
                var componentAndFilterID = Internal_FilterHelper.CombineFilterIDWithComponentID<T>(combinedFilterID);

                if (_transientEntityFilters.TryFindIndex(componentAndFilterID, out var index))
                    return _transientEntityFilters.GetDirectValueByRef(index);

                return InternalCreateTransientFilter<T>(combinedFilterID, componentAndFilterID, trackFilter);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollection GetOrCreateTransientFilter<T>(int filterID, FilterContextID filterContextId)
                    where T : struct, _IInternalEntityComponent
            {
                return GetOrCreateTransientFilter<T>(new CombinedFilterID(filterID, filterContextId));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollection CreateTransientFilter<T>(CombinedFilterID combinedFilterID, bool trackFilter = false)
                    where T : struct, _IInternalEntityComponent
            {
                CombinedFilterComponentID componentAndFilterID = Internal_FilterHelper.CombineFilterIDWithComponentID<T>(combinedFilterID);
#if DEBUG && !PROFILE_SVELTO
                if (_transientEntityFilters.TryFindIndex(componentAndFilterID, out _))
                    throw new ECSException($"filter already exists {TypeCache<T>.name}");
#endif
                return InternalCreateTransientFilter<T>(combinedFilterID, componentAndFilterID, trackFilter);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetTransientFilter<T>(CombinedFilterID filterID, out EntityFilterCollection entityCollection)
                    where T : struct, _IInternalEntityComponent
            {
                var componentAndFilterID = Internal_FilterHelper.CombineFilterIDWithComponentID<T>(filterID);

                if (_transientEntityFilters.TryFindIndex(componentAndFilterID, out var index))
                {
                    entityCollection = _transientEntityFilters.GetDirectValueByRef(index);
                    return true;
                }

                entityCollection = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollection GetTransientFilter<T>(CombinedFilterID filterID)
                    where T : struct, _IInternalEntityComponent
            {
                var componentAndFilterID = Internal_FilterHelper.CombineFilterIDWithComponentID<T>(filterID);

                if (_transientEntityFilters.TryFindIndex(componentAndFilterID, out var index))
                {
                    return _transientEntityFilters.GetDirectValueByRef(index);
                }

                throw new ECSException($"no filters associated with the type {TypeCache<T>.name}");
            }

            /// <summary>
            /// Svelto.ECS tracks the filters linked to each
            /// component. This allows to iterate over all the filters of a given filter context linked to a component.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollectionsEnumerator GetTransientFilters<T>()
                    where T : struct, _IInternalEntityComponent
            {
                if (_indicesOfTransientFiltersUsedByThisComponent.TryFindIndex(ComponentTypeID<T>.id, out var index) == true)
                    return new EntityFilterCollectionsEnumerator(
                        _indicesOfTransientFiltersUsedByThisComponent.GetDirectValueByRef(index),
                        _transientEntityFilters);

                throw new ECSException($"no filters associated with the type {TypeCache<T>.name}");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public EntityFilterCollectionsWithContextEnumerator GetTransientFilters<T>(FilterContextID filterContextId)
                    where T : struct, _IInternalEntityComponent
            {
                if (_indicesOfTransientFiltersUsedByThisComponent.TryFindIndex(ComponentTypeID<T>.id, out var index) == true)
                    return new EntityFilterCollectionsWithContextEnumerator(
                        _indicesOfTransientFiltersUsedByThisComponent.GetDirectValueByRef(index),
                        _transientEntityFilters, filterContextId);

                throw new ECSException($"no filters associated with the type {TypeCache<T>.name}");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool TryGetTransientFilters<T>(FilterContextID filterContextId, out EntityFilterCollectionsWithContextEnumerator enumerator)
                    where T : struct, _IInternalEntityComponent
            {
                if (_indicesOfTransientFiltersUsedByThisComponent.TryFindIndex(ComponentTypeID<T>.id, out var index) == true)
                {
                    enumerator = new EntityFilterCollectionsWithContextEnumerator(
                        _indicesOfTransientFiltersUsedByThisComponent.GetDirectValueByRef(index),
                        _transientEntityFilters, filterContextId);

                    return true;
                }

                enumerator = default;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            EntityFilterCollection InternalCreateTransientFilter<T>(CombinedFilterID filterID, CombinedFilterComponentID componentAndFilterID,
                bool trackFilter)
                    where T : struct, _IInternalEntityComponent
            {
                var filterCollection = new EntityFilterCollection(filterID);

                _transientEntityFilters.Add(componentAndFilterID, filterCollection);

                if (trackFilter)
                {
                    var lastIndex = _transientEntityFilters.count - 1;
                    _indicesOfTransientFiltersUsedByThisComponent.GetOrAdd(ComponentTypeID<T>.id, _builder).Add(lastIndex);
                }

                return _transientEntityFilters.GetDirectValueByRef((uint)(_transientEntityFilters.count - 1));
            }

            public struct EntityFilterCollectionsEnumerator
            {
                internal EntityFilterCollectionsEnumerator(NativeDynamicArrayCast<int> getDirectValueByRef,
                    SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection> sharedSveltoDictionaryNative): this()
                {
                    _getDirectValueByRef = getDirectValueByRef;
                    _sharedSveltoDictionaryNative = sharedSveltoDictionaryNative;
                }

                public EntityFilterCollectionsEnumerator GetEnumerator()
                {
                    return this;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    if (_currentIndex < _getDirectValueByRef.count)
                    {
                        _currentIndex++;

                        return true;
                    }

                    return false;
                }

                public EntityFilterCollection Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => _sharedSveltoDictionaryNative.GetDirectValueByRef((uint)_currentIndex - 1);
                }

                readonly NativeDynamicArrayCast<int> _getDirectValueByRef;
                readonly SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection> _sharedSveltoDictionaryNative;
                int _currentIndex;
            }

            /// <summary>
            /// TODO: ABSOLUTELY UNIT TEST THIS AS THE CODE WAS WRONG!!!
            /// </summary>
            public struct EntityFilterCollectionsWithContextEnumerator
            {
                internal EntityFilterCollectionsWithContextEnumerator(NativeDynamicArrayCast<int> filterIndices,
                    SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection> sharedSveltoDictionaryNative,
                    FilterContextID filterContextId): this()
                {
                    _filterIndices = filterIndices;
                    _sharedSveltoDictionaryNative = sharedSveltoDictionaryNative;
                    _filterContextId = filterContextId;
                }

                public EntityFilterCollectionsWithContextEnumerator GetEnumerator()
                {
                    return this;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public bool MoveNext()
                {
                    while (_currentIndex++ < _filterIndices.count &&
                           _sharedSveltoDictionaryNative.GetDirectValueByRef((uint)_filterIndices[(uint)_currentIndex - 1]).combinedFilterID
                                  .contextID.id != _filterContextId.id);

                    if (_currentIndex - 1 < _filterIndices.count)
                        return true;

                    return false;
                }

                public ref EntityFilterCollection Current
                {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => ref _sharedSveltoDictionaryNative.GetDirectValueByRef((uint)_filterIndices[(uint)_currentIndex - 1]);
                }

                readonly NativeDynamicArrayCast<int> _filterIndices;
                readonly SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection> _sharedSveltoDictionaryNative;
                readonly FilterContextID _filterContextId;
                int _currentIndex;
            }

            readonly SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection> _persistentEntityFilters;

            readonly SharedSveltoDictionaryNative<ComponentID, NativeDynamicArrayCast<int>>
                    _indicesOfPersistentFiltersUsedByThisComponent;

            readonly SharedSveltoDictionaryNative<CombinedFilterComponentID, EntityFilterCollection> _transientEntityFilters;

            readonly SharedSveltoDictionaryNative<ComponentID, NativeDynamicArrayCast<int>>
                    _indicesOfTransientFiltersUsedByThisComponent;

            static readonly Func<NativeDynamicArrayCast<int>> _builder = Builder;
        }
    }
}