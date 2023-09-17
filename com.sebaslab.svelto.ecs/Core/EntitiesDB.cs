#if DEBUG && !PROFILE_SVELTO
#define ENABLE_DEBUG_FUNC
#endif

using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        internal EntitiesDB(EnginesRoot enginesRoot, EnginesRoot.EntityReferenceMap entityReferencesMap)
        {
            _enginesRoot = enginesRoot;
            _entityReferencesMap = entityReferencesMap;
        }

        public void PreallocateEntitySpace<T>(ExclusiveGroupStruct groupStructId, uint numberOfEntities)
                where T : IEntityDescriptor, new()
        {
            _enginesRoot.Preallocate(groupStructId, numberOfEntities, EntityDescriptorTemplate<T>.realDescriptor.componentsToBuild);
        }

        /// <summary>
        /// The QueryEntities<T> follows the rule that entities could always be iterated regardless if they
        /// are 0, 1 or N. In case of 0 it returns an empty array. This allows to use the same for iteration
        /// regardless the number of entities built.
        /// </summary>
        /// <param name="groupStructId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public EntityCollection<T> QueryEntities<T>(ExclusiveGroupStruct groupStructId)
                where T : struct, _IInternalEntityComponent
        {
            if (groupEntityComponentsDB.TryGetValue(groupStructId, out var entitiesInGroupPerType) == false)
            {
                return new EntityCollection<T>(default, default, 0);
            }

            return InternalQueryEntities<T>(entitiesInGroupPerType);
        }

        public EntityCollection<T1, T2> QueryEntities<T1, T2>(ExclusiveGroupStruct groupStruct)
                where T1 : struct, _IInternalEntityComponent where T2 : struct, _IInternalEntityComponent
        {
            if (groupEntityComponentsDB.TryGetValue(groupStruct, out var entitiesInGroupPerType) == false)
            {
                return new EntityCollection<T1, T2>(
                    new EntityCollection<T1>(default, default, 0)
                  , new EntityCollection<T2>(default, default, 0));
            }

            var T1entities = InternalQueryEntities<T1>(entitiesInGroupPerType);
            var T2entities = InternalQueryEntities<T2>(entitiesInGroupPerType);
#if DEBUG && !PROFILE_SVELTO
            if (T1entities.count != T2entities.count)
                throw new ECSException(
                    "Entity components count do not match in group. Entity 1: ' count: "
                           .FastConcat(T1entities.count).FastConcat(" ", typeof(T1).ToString())
                           .FastConcat(
                                "'. Entity 2: ' count: ".FastConcat(T2entities.count)
                                       .FastConcat(" ", typeof(T2).ToString())
                                       .FastConcat("' group: ", groupStruct.ToName())).FastConcat(
                                " this means that you are mixing descriptors in the same group that do not share the components that you are querying"));
#endif

            return new EntityCollection<T1, T2>(T1entities, T2entities);
        }

        public EntityCollection<T1, T2, T3> QueryEntities<T1, T2, T3>(ExclusiveGroupStruct groupStruct)
                where T1 : struct, _IInternalEntityComponent
                where T2 : struct, _IInternalEntityComponent
                where T3 : struct, _IInternalEntityComponent
        {
            if (groupEntityComponentsDB.TryGetValue(groupStruct, out var entitiesInGroupPerType) == false)
            {
                return new EntityCollection<T1, T2, T3>(
                    new EntityCollection<T1>(default, default, 0)
                  , new EntityCollection<T2>(default, default, 0)
                  , new EntityCollection<T3>(default, default, 0));
            }

            var T1entities = InternalQueryEntities<T1>(entitiesInGroupPerType);
            var T2entities = InternalQueryEntities<T2>(entitiesInGroupPerType);
            var T3entities = InternalQueryEntities<T3>(entitiesInGroupPerType);
#if DEBUG && !PROFILE_SVELTO
            if (T1entities.count != T2entities.count || T2entities.count != T3entities.count)
                throw new ECSException(
                    "Entity components count do not match in group. Entity 1: "
                           .FastConcat(typeof(T1).ToString()).FastConcat(" count: ")
                           .FastConcat(T1entities.count).FastConcat(
                                " Entity 2: ".FastConcat(typeof(T2).ToString()).FastConcat(" count: ")
                                       .FastConcat(T2entities.count)
                                       .FastConcat(" Entity 3: ".FastConcat(typeof(T3).ToString()))
                                       .FastConcat(" count: ").FastConcat(T3entities.count)).FastConcat(
                                " this means that you are mixing descriptors in the same group that do not share the components that you are querying"));
#endif

            return new EntityCollection<T1, T2, T3>(T1entities, T2entities, T3entities);
        }

        public EntityCollection<T1, T2, T3, T4> QueryEntities<T1, T2, T3, T4>(ExclusiveGroupStruct groupStruct)
                where T1 : struct, _IInternalEntityComponent
                where T2 : struct, _IInternalEntityComponent
                where T3 : struct, _IInternalEntityComponent
                where T4 : struct, _IInternalEntityComponent
        {
            if (groupEntityComponentsDB.TryGetValue(groupStruct, out var entitiesInGroupPerType) == false)
            {
                return new EntityCollection<T1, T2, T3, T4>(
                    new EntityCollection<T1>(default, default, 0)
                  , new EntityCollection<T2>(default, default, 0)
                  , new EntityCollection<T3>(default, default, 0)
                  , new EntityCollection<T4>(default, default, 0));
            }

            var T1entities = InternalQueryEntities<T1>(entitiesInGroupPerType);
            var T2entities = InternalQueryEntities<T2>(entitiesInGroupPerType);
            var T3entities = InternalQueryEntities<T3>(entitiesInGroupPerType);
            var T4entities = InternalQueryEntities<T4>(entitiesInGroupPerType);
#if DEBUG && !PROFILE_SVELTO
            if (T1entities.count != T2entities.count || T2entities.count != T3entities.count
             || T3entities.count != T4entities.count)
                throw new ECSException(
                    "Entity components count do not match in group. Entity 1: "
                           .FastConcat(typeof(T1).ToString()).FastConcat(" count: ")
                           .FastConcat(T1entities.count).FastConcat(
                                " Entity 2: ".FastConcat(typeof(T2).ToString()).FastConcat(" count: ")
                                       .FastConcat(T2entities.count)
                                       .FastConcat(" Entity 3: ".FastConcat(typeof(T3).ToString()))
                                       .FastConcat(" count: ").FastConcat(T3entities.count)
                                       .FastConcat(" Entity 4: ".FastConcat(typeof(T4).ToString()))
                                       .FastConcat(" count: ").FastConcat(T4entities.count)).FastConcat(
                                " this means that you are mixing descriptors in the same group that do not share the components that you are querying"));
#endif

            return new EntityCollection<T1, T2, T3, T4>(T1entities, T2entities, T3entities, T4entities);
        }

        public GroupsEnumerable<T> QueryEntities<T>(in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
                where T : struct, _IInternalEntityComponent
        {
            return new GroupsEnumerable<T>(this, groups);
        }

        /// <summary>
        /// Note: Remember that EntityViewComponents are always put at the end of the generic parameters tuple.
        /// The Query entity code won't inexplicably compile otherwise
        /// </summary>
        /// <returns></returns>
        public GroupsEnumerable<T1, T2> QueryEntities<T1, T2>(in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
                where T1 : struct, _IInternalEntityComponent where T2 : struct, _IInternalEntityComponent
        {
            return new GroupsEnumerable<T1, T2>(this, groups);
        }

        public GroupsEnumerable<T1, T2, T3> QueryEntities<T1, T2, T3>(in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
                where T1 : struct, _IInternalEntityComponent
                where T2 : struct, _IInternalEntityComponent
                where T3 : struct, _IInternalEntityComponent
        {
            return new GroupsEnumerable<T1, T2, T3>(this, groups);
        }

        public GroupsEnumerable<T1, T2, T3, T4> QueryEntities<T1, T2, T3, T4>(in LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
                where T1 : struct, _IInternalEntityComponent
                where T2 : struct, _IInternalEntityComponent
                where T3 : struct, _IInternalEntityComponent
                where T4 : struct, _IInternalEntityComponent
        {
            return new GroupsEnumerable<T1, T2, T3, T4>(this, groups);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EGIDMapper<T> QueryMappedEntities<T>(ExclusiveGroupStruct groupStructId)
                where T : struct, _IInternalEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false)
                throw new EntityGroupNotFoundException(typeof(T), groupStructId.ToName());

            return (typeSafeDictionary as ITypeSafeDictionary<T>).ToEGIDMapper(groupStructId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryQueryMappedEntities<T>(ExclusiveGroupStruct groupStructId, out EGIDMapper<T> mapper)
                where T : struct, _IInternalEntityComponent
        {
            mapper = default;
            if (SafeQueryEntityDictionary<T>(groupStructId, out var typeSafeDictionary) == false
             || typeSafeDictionary.count == 0)
                return false;

            mapper = (typeSafeDictionary as ITypeSafeDictionary<T>).ToEGIDMapper(groupStructId);

            return true;
        }

        /// <summary>
        /// determine if component with specific ID exists in group
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(EGID entityGID) where T : struct, _IInternalEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(entityGID.groupID, out var casted) == false)
                return false;

            return casted != null && casted.ContainsKey(entityGID.entityID);
        }

        /// <summary>
        /// determine if component with specific ID exists in group
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Exists<T>(uint id, ExclusiveGroupStruct group) where T : struct, _IInternalEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(group, out var casted) == false)
                return false;

            return casted != null && casted.ContainsKey(id);
        }

        /// <summary>
        /// determine if group exists and is not empty
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ExistsAndIsNotEmpty(ExclusiveGroupStruct gid)
        {
            if (groupEntityComponentsDB.TryGetValue(gid, out FasterDictionary<ComponentID, ITypeSafeDictionary> group) == true)
            {
                return group.count > 0;
            }

            return false;
        }

        /// <summary>
        /// determine if entities we specific components are found in group
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAny<T>(ExclusiveGroupStruct groupStruct) where T : struct, _IInternalEntityComponent
        {
            return Count<T>(groupStruct) > 0;
        }

        /// <summary>
        /// count the number of components in a group
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T>(ExclusiveGroupStruct groupStruct) where T : struct, _IInternalEntityComponent
        {
            if (SafeQueryEntityDictionary<T>(groupStruct, out var typeSafeDictionary) == false)
                return 0;

            return (int)typeSafeDictionary.count;
        }

        public bool FoundInGroups<T>() where T : struct, _IInternalEntityComponent
        {
            return groupsPerComponent.ContainsKey(ComponentTypeID<T>.id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool SafeQueryEntityDictionary<T>(FasterDictionary<ComponentID, ITypeSafeDictionary> entitiesInGroupPerType,
            out ITypeSafeDictionary typeSafeDictionary) where T : struct, _IInternalEntityComponent
        {
            if (entitiesInGroupPerType.TryGetValue(ComponentTypeID<T>.id, out var safeDictionary)
             == false)
            {
                typeSafeDictionary = default;
                return false;
            }

            //return the indexes entities if they exist
            typeSafeDictionary = safeDictionary;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool SafeQueryEntityDictionary<T>(ExclusiveGroupStruct group, out ITypeSafeDictionary typeSafeDictionary)
                where T : struct, _IInternalEntityComponent
        {
            ITypeSafeDictionary safeDictionary;
            bool ret;
            //search for the group
            if (groupEntityComponentsDB.TryGetValue(group, out FasterDictionary<ComponentID, ITypeSafeDictionary> entitiesInGroupPerType) == false)
            {
                safeDictionary = null;
                ret = false;
            }
            else
            {
                ret = entitiesInGroupPerType.TryGetValue(ComponentTypeID<T>.id, out safeDictionary);
            }

            //search for the indexed entities in the group
            if (ret == false)
            {
                typeSafeDictionary = default;
                return false;
            }

            //return the indexes entities if they exist
            typeSafeDictionary = safeDictionary;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void QueryOrCreateEntityDictionary<T>(ExclusiveGroupStruct group, out ITypeSafeDictionary typeSafeDictionary)
                where T : struct, _IInternalEntityComponent
        {
            //search for the group
            FasterDictionary<ComponentID, ITypeSafeDictionary> entitiesInGroupPerType =
                    groupEntityComponentsDB.GetOrAdd(group, () => new FasterDictionary<ComponentID, ITypeSafeDictionary>());

            typeSafeDictionary = entitiesInGroupPerType.GetOrAdd(ComponentTypeID<T>.id, () => TypeSafeDictionaryFactory<T>.Create(0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool UnsafeQueryEntityDictionary(ExclusiveGroupStruct groupID, ComponentID id, out ITypeSafeDictionary typeSafeDictionary)
        {
            //search for the group
            if (groupEntityComponentsDB.TryGetValue(groupID, out FasterDictionary<ComponentID, ITypeSafeDictionary> entitiesInGroupPerType) == false)
            {
                typeSafeDictionary = null;
                return false;
            }

            //search for the indexed entities in the group
            return entitiesInGroupPerType.TryGetValue(id, out typeSafeDictionary);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityCollection<T> InternalQueryEntities<T>(FasterDictionary<ComponentID, ITypeSafeDictionary> entitiesInGroupPerType)
                where T : struct, _IInternalEntityComponent
        {
            uint count = 0;
            IBuffer<T> buffer;
            IEntityIDs ids = default;

            if (SafeQueryEntityDictionary<T>(entitiesInGroupPerType, out var typeSafeDictionary) == false)
                buffer = default;
            else
            {
                ITypeSafeDictionary<T> safeDictionary = (typeSafeDictionary as ITypeSafeDictionary<T>);
                buffer = safeDictionary.GetValues(out count);
                ids = safeDictionary.entityIDs;
            }

            return new EntityCollection<T>(buffer, ids, count);
        }

        static readonly FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary> _emptyDictionary =
                new FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>();

        readonly EnginesRoot _enginesRoot;

        EntitiesStreams _entityStream => _enginesRoot._entityStreams;

        //grouped set of entity components, this is the standard way to handle entity components are grouped per
        //group, then indexable per type, then indexable per EGID. however the TypeSafeDictionary can return an array of
        //values directly, that can be iterated over, so that is possible to iterate over all the entity components of
        //a specific type inside a specific group.
        FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>
                groupEntityComponentsDB => _enginesRoot._groupEntityComponentsDB;

        //for each entity view type, return the groups (dictionary of entities indexed by entity id) where they are
        //found indexed by group id. TypeSafeDictionary are never created, they instead point to the ones hold
        //by _groupEntityComponentsDB
        //                        <EntityComponentType                            <groupID  <entityID, EntityComponent>>>
        FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>> groupsPerComponent =>
                _enginesRoot._groupsPerEntity;

        EnginesRoot.EntityReferenceMap _entityReferencesMap;
    }
}