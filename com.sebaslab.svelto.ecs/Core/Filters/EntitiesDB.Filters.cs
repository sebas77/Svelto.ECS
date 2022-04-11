using System;
using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;


namespace Svelto.ECS
{
    public struct FilterContextID
    {
        public readonly uint id;

        internal FilterContextID(uint id)
        {
            DBC.ECS.Check.Require(id < ushort.MaxValue, "too many types registered, HOW :)");

            this.id = id;
        }
    }

    public readonly struct CombinedFilterID
    {
        internal readonly long            id;
        
        public          FilterContextID contextID => new FilterContextID((uint)((id & 0xFFFF0000) >> 16));
        public          uint            filterID   => (uint)(id >> 32);

        public CombinedFilterID(int filterID, FilterContextID contextID)
        {
            id = (long)filterID << 32 | (uint)contextID.id << 16;
        }

        public static implicit operator CombinedFilterID((int filterID, FilterContextID contextID) data)
        {
            return new CombinedFilterID(data.filterID, data.contextID);
        }
    }
    
    //this cannot be inside EntitiesDB otherwise it will cause hashing of reference in Burst
    public class Internal_FilterHelper
    {
        //since the user can choose their own filterID, in order to avoid collisions between
        //filters of the same type, the FilterContext is provided. The type is identified through
        //TypeCounter
        public static long CombineFilterIDs<T>(CombinedFilterID combinedFilterID) where T: struct, IEntityComponent
        {
            var id = (uint)ComponentID<T>.id.Data;

            var combineFilterIDs = (long)combinedFilterID.id | id;

            return combineFilterIDs;
        }
    }

    public partial class EntitiesDB
    {
        public SveltoFilters GetFilters()
        {
            return new SveltoFilters(_enginesRoot._persistentEntityFilters,
                _enginesRoot._indicesOfPersistentFiltersUsedByThisComponent, _enginesRoot._transientEntityFilters);
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
                return new FilterContextID((uint)Interlocked.Increment(ref uniqueContextID.Data));
            }

            public SveltoFilters(SharedSveltoDictionaryNative<long, EntityFilterCollection> persistentEntityFilters,
                SharedSveltoDictionaryNative<NativeRefWrapperType, NativeDynamicArrayCast<int>>
                    indicesOfPersistentFiltersUsedByThisComponent,
                SharedSveltoDictionaryNative<long, EntityFilterCollection> transientEntityFilters)
            {
                _persistentEntityFilters                       = persistentEntityFilters;
                _indicesOfPersistentFiltersUsedByThisComponent = indicesOfPersistentFiltersUsedByThisComponent;
                _transientEntityFilters                        = transientEntityFilters;
            }
            
#if UNITY_BURST
            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(int filterID,
                FilterContextID filterContextId, NativeRefWrapperType typeRef) where T : unmanaged, IEntityComponent
            {
                return ref GetOrCreatePersistentFilter<T>(new CombinedFilterID(filterID, filterContextId), typeRef);
            }

            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(CombinedFilterID filterID,
                NativeRefWrapperType typeRef) where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs = Internal_FilterHelper.CombineFilterIDs<T>(filterID);
                
                if (_persistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref _persistentEntityFilters.GetDirectValueByRef(index);

                _persistentEntityFilters.Add(combineFilterIDs, new EntityFilterCollection(filterID));

                var lastIndex = _persistentEntityFilters.count - 1;

                if (_indicesOfPersistentFiltersUsedByThisComponent.TryFindIndex(typeRef, out var getIndex) == false)
                {
                    var newArray = new NativeDynamicArrayCast<int>(1, Allocator.Persistent);
                    newArray.Add(lastIndex);
                    _indicesOfPersistentFiltersUsedByThisComponent.Add(typeRef, newArray);
                }
                else
                {
                    ref var array = ref _indicesOfPersistentFiltersUsedByThisComponent.GetDirectValueByRef(getIndex);
                    array.Add(lastIndex);
                }

                return ref _persistentEntityFilters.GetDirectValueByRef((uint)lastIndex);
            }
#endif

            /// <summary>
            /// Create a persistent filter. Persistent filters are not deleted after each submission,
            /// however they have a maintenance cost that must be taken into account and will affect
            /// entities submission performance.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
#if UNITY_BURST && UNITY_COLLECTIONS
            [Unity.Collections.NotBurstCompatible]
#endif
            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(int filterID, FilterContextID filterContextId)
                where T : unmanaged, IEntityComponent
            {
                return ref GetOrCreatePersistentFilter<T>(new CombinedFilterID(filterID, filterContextId));
            }
#if UNITY_BURST && UNITY_COLLECTIONS
            [Unity.Collections.NotBurstCompatible]
#endif
            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(CombinedFilterID filterID)
                where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs = Internal_FilterHelper.CombineFilterIDs<T>(filterID);
                
                if (_persistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref _persistentEntityFilters.GetDirectValueByRef(index);

                var typeRef          = TypeRefWrapper<T>.wrapper;
                var filterCollection = new EntityFilterCollection(filterID);

                _persistentEntityFilters.Add(combineFilterIDs, filterCollection);

                var lastIndex = _persistentEntityFilters.count - 1;

                _indicesOfPersistentFiltersUsedByThisComponent.GetOrAdd(new NativeRefWrapperType(typeRef),
                    () => new NativeDynamicArrayCast<int>(1, Svelto.Common.Allocator.Persistent)).Add(lastIndex);

                return ref _persistentEntityFilters.GetDirectValueByRef((uint)lastIndex);
            }

            public ref EntityFilterCollection GetPersistentFilter<T>(int filterID, FilterContextID filterContextId)
                where T : unmanaged, IEntityComponent
            {
                return ref GetPersistentFilter<T>(new CombinedFilterID(filterID, filterContextId));
            }

            public ref EntityFilterCollection GetPersistentFilter<T>(CombinedFilterID filterID)
                where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs = Internal_FilterHelper.CombineFilterIDs<T>(filterID);
                
                if (_persistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref _persistentEntityFilters.GetDirectValueByRef(index);

                throw new Exception("filter not found");
            }
            
            public bool TryGetPersistentFilter<T>(CombinedFilterID combinedFilterID, out EntityFilterCollection entityCollection) 
                where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs = Internal_FilterHelper.CombineFilterIDs<T>(combinedFilterID);
                
                if (_persistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                {
                    entityCollection = _persistentEntityFilters.GetDirectValueByRef(index);
                    return true;
                }

                entityCollection = default;
                return false;
            }

            public EntityFilterCollectionsEnumerator GetPersistentFilters<T>() where T : unmanaged, IEntityComponent
            {
                if (_indicesOfPersistentFiltersUsedByThisComponent.TryFindIndex(
                        new NativeRefWrapperType(new RefWrapperType(typeof(T))), out var index) == true)
                    return new EntityFilterCollectionsEnumerator(
                        _indicesOfPersistentFiltersUsedByThisComponent.GetDirectValueByRef(index),
                        _persistentEntityFilters);

                throw new Exception($"no filters associated with the type {TypeCache<T>.name}");
            }
            
            public EntityFilterCollectionsWithContextEnumerator GetPersistentFilters<T>(FilterContextID filterContextId)
            {
                if (_indicesOfPersistentFiltersUsedByThisComponent.TryFindIndex(
                        new NativeRefWrapperType(new RefWrapperType(typeof(T))), out var index) == true)
                    return new EntityFilterCollectionsWithContextEnumerator(
                        _indicesOfPersistentFiltersUsedByThisComponent.GetDirectValueByRef(index),
                        _persistentEntityFilters, filterContextId);

                throw new Exception($"no filters associated with the type {TypeCache<T>.name}");
            }
            
            public bool TryGetPersistentFilters<T>(FilterContextID filterContextId, out EntityFilterCollectionsWithContextEnumerator enumerator)
            {
                if (_indicesOfPersistentFiltersUsedByThisComponent.TryFindIndex(
                        new NativeRefWrapperType(new RefWrapperType(typeof(T))), out var index) == true)
                {
                    enumerator = new EntityFilterCollectionsWithContextEnumerator(
                        _indicesOfPersistentFiltersUsedByThisComponent.GetDirectValueByRef(index),
                        _persistentEntityFilters, filterContextId);

                    return true;
                }

                enumerator = default;
                return false;
            }

            public struct EntityFilterCollectionsEnumerator
            {
                public EntityFilterCollectionsEnumerator(NativeDynamicArrayCast<int> getDirectValueByRef,
                    SharedSveltoDictionaryNative<long, EntityFilterCollection> sharedSveltoDictionaryNative) : this()
                {
                    _getDirectValueByRef          = getDirectValueByRef;
                    _sharedSveltoDictionaryNative = sharedSveltoDictionaryNative;
                }

                public EntityFilterCollectionsEnumerator GetEnumerator()
                {
                    return this;
                }

                public bool MoveNext()
                {
                    if (_currentIndex < _getDirectValueByRef.count)
                    {
                        _currentIndex++;
                        
                        return true;
                    }

                    return false;
                }

                public ref EntityFilterCollection Current =>
                    ref _sharedSveltoDictionaryNative.GetDirectValueByRef((uint)_currentIndex - 1);

                readonly NativeDynamicArrayCast<int>                                _getDirectValueByRef;
                readonly SharedSveltoDictionaryNative<long, EntityFilterCollection> _sharedSveltoDictionaryNative;
                int                                                                 _currentIndex;
            }
            
            public struct EntityFilterCollectionsWithContextEnumerator
            {
                public EntityFilterCollectionsWithContextEnumerator(NativeDynamicArrayCast<int> getDirectValueByRef,
                    SharedSveltoDictionaryNative<long, EntityFilterCollection> sharedSveltoDictionaryNative,
                    FilterContextID filterContextId) : this()
                {
                    _getDirectValueByRef          = getDirectValueByRef;
                    _sharedSveltoDictionaryNative = sharedSveltoDictionaryNative;
                    _filterContextId         = filterContextId;
                }
            
                public EntityFilterCollectionsWithContextEnumerator GetEnumerator()
                {
                    return this;
                }
            
                public bool MoveNext()
                {
                    while (_currentIndex++ < _getDirectValueByRef.count &&
                           _sharedSveltoDictionaryNative.GetDirectValueByRef((uint)_currentIndex - 1).combinedFilterID
                              .contextID.id != _filterContextId.id) ;
                            
                    if (_currentIndex - 1 < _getDirectValueByRef.count) 
                        return true;
                    
                    return false;
                }
            
                public ref EntityFilterCollection Current =>
                    ref _sharedSveltoDictionaryNative.GetDirectValueByRef((uint)_currentIndex - 1);
            
                readonly NativeDynamicArrayCast<int>                                _getDirectValueByRef;
                readonly SharedSveltoDictionaryNative<long, EntityFilterCollection> _sharedSveltoDictionaryNative;
                readonly FilterContextID                                            _filterContextId;
                int                                                                 _currentIndex;
            }

            /// <summary>
            /// Creates a transient filter. Transient filters are deleted after each submission
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public ref EntityFilterCollection GetOrCreateTransientFilter<T>(CombinedFilterID filterID)
                where T : unmanaged, IEntityComponent
            {
                var combineFilterIDs = Internal_FilterHelper.CombineFilterIDs<T>(filterID);

                if (_transientEntityFilters.TryFindIndex(combineFilterIDs, out var index))
                    return ref _transientEntityFilters.GetDirectValueByRef(index);

                var filterCollection = new EntityFilterCollection(filterID);

                _transientEntityFilters.Add(combineFilterIDs, filterCollection);

                return ref _transientEntityFilters.GetDirectValueByRef((uint)(_transientEntityFilters.count - 1));
            }

            public bool TryGetTransientFilter<T>(CombinedFilterID filterID, out EntityFilterCollection entityCollection)
                where T : unmanaged, IEntityComponent
            {
                var combineFilterIDs = Internal_FilterHelper.CombineFilterIDs<T>(filterID);

                if (_transientEntityFilters.TryFindIndex(combineFilterIDs, out var index))
                {
                    entityCollection = _transientEntityFilters.GetDirectValueByRef(index);
                    return true;
                }

                entityCollection = default;
                return false;
            }

            readonly SharedSveltoDictionaryNative<long, EntityFilterCollection> _persistentEntityFilters;

            readonly SharedSveltoDictionaryNative<NativeRefWrapperType, NativeDynamicArrayCast<int>>
                _indicesOfPersistentFiltersUsedByThisComponent;

            readonly SharedSveltoDictionaryNative<long, EntityFilterCollection> _transientEntityFilters;
        }
    }
}