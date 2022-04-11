//#define PARANOID_CHECK

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot : IDisposable, IUnitTestingInterface
    {
        ///--------------------------------------------
        ///
        public IEntityStreamConsumerFactory GenerateConsumerFactory()
        {
            return new GenericEntityStreamConsumerFactory(this);
        }

        public IEntityFactory GenerateEntityFactory()
        {
            return new GenericEntityFactory(this);
        }

        public IEntityFunctions GenerateEntityFunctions()
        {
            return new GenericEntityFunctions(this);
        }

        ///--------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityInitializer BuildEntity(EGID entityID, IComponentBuilder[] componentsToBuild, Type descriptorType,
            IEnumerable<object> implementors, string caller)
        {
            CheckAddEntityID(entityID, descriptorType, caller);

            DBC.ECS.Check.Require(entityID.groupID.isInvalid == false,
                "invalid group detected, are you using new ExclusiveGroupStruct() instead of new ExclusiveGroup()?");

            var reference = _entityLocator.ClaimReference();
            _entityLocator.SetReference(reference, entityID);

            var dic = EntityFactory.BuildGroupedEntities(entityID, _groupedEntityToAdd, componentsToBuild, implementors
#if DEBUG && !PROFILE_SVELTO
              , descriptorType
#endif
            );

            return new EntityInitializer(entityID, dic, reference);
        }

        /// <summary>
        /// Preallocate memory to avoid the impact to resize arrays when many entities are submitted at once
        /// </summary>
        void Preallocate(ExclusiveGroupStruct groupID, uint size, IComponentBuilder[] entityComponentsToBuild)
        {
            void PreallocateEntitiesToAdd()
            {
                _groupedEntityToAdd.Preallocate(groupID, size, entityComponentsToBuild);
            }

            void PreallocateDBGroup()
            {
                var numberOfEntityComponents = entityComponentsToBuild.Length;
                FasterDictionary<RefWrapperType, ITypeSafeDictionary> group = GetOrAddDBGroup(groupID);

                for (var index = 0; index < numberOfEntityComponents; index++)
                {
                    var entityComponentBuilder = entityComponentsToBuild[index];
                    var entityComponentType    = entityComponentBuilder.GetEntityComponentType();

                    var refWrapper = new RefWrapperType(entityComponentType);
                    var dbList     = group.GetOrAdd(refWrapper, () => entityComponentBuilder.CreateDictionary(size));
                    entityComponentBuilder.Preallocate(dbList, size);

                    if (_groupsPerEntity.TryGetValue(refWrapper, out var groupedGroup) == false)
                        groupedGroup = _groupsPerEntity[refWrapper] =
                            new FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>();

                    groupedGroup[groupID] = dbList;
                }
            }

            PreallocateDBGroup();
            PreallocateEntitiesToAdd();
            _entityLocator.PreallocateReferenceMaps(groupID, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        FasterDictionary<RefWrapperType, ITypeSafeDictionary> GetDBGroup(ExclusiveGroupStruct fromIdGroupId)
        {
            if (_groupEntityComponentsDB.TryGetValue(fromIdGroupId,
                    out FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup) == false)
                throw new ECSException("Group doesn't exist: ".FastConcat(fromIdGroupId.ToName()));

            return fromGroup;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        FasterDictionary<RefWrapperType, ITypeSafeDictionary> GetOrAddDBGroup(ExclusiveGroupStruct toGroupId)
        {
            return _groupEntityComponentsDB.GetOrAdd(toGroupId,
                () => new FasterDictionary<RefWrapperType, ITypeSafeDictionary>());
        }

        IComponentBuilder[] FindRealComponents<T>(EGID fromEntityGID) where T : IEntityDescriptor, new()
        {
            var fromGroup = GetDBGroup(fromEntityGID.groupID);

            if (fromGroup.TryGetValue(new RefWrapperType(ComponentBuilderUtilities.ENTITY_INFO_COMPONENT),
                    out var entityInfoDic) //<entity ID, EntityInfoComponent>
             && ((ITypeSafeDictionary<EntityInfoComponent>)entityInfoDic).TryGetValue(fromEntityGID.entityID,
                    out var entityInfo)) //there could be multiple entity descriptors registered in the same group, so it's necessary to check if the entity registered in the group has entityInfoComponent   
            {
#if PARANOID_CHECK
                var hash = new HashSet<IComponentBuilder>(entityInfo.componentsToBuild,
                    default(ComponentBuilderComparer));

                foreach (var component in EntityDescriptorTemplate<T>.descriptor.componentsToBuild)
                {
                    if (hash.Contains(component) == false)
                        throw new Exception(
                            $"entityInfo.componentsToBuild must contain all the base components {fromEntityGID}," +
                            $" missing component {component}");

                    hash.Remove(component);
                }
#endif
                return entityInfo.componentsToBuild;
            }

            return EntityDescriptorTemplate<T>.descriptor.componentsToBuild;
        }

        IComponentBuilder[] FindRealComponents(EGID fromEntityGID, IComponentBuilder[] baseComponents)
        {
            var fromGroup = GetDBGroup(fromEntityGID.groupID);

            if (fromGroup.TryGetValue(new RefWrapperType(ComponentBuilderUtilities.ENTITY_INFO_COMPONENT),
                    out var entityInfoDic) //<entity ID, EntityInfoComponent>
             && ((ITypeSafeDictionary<EntityInfoComponent>)entityInfoDic).TryGetValue(fromEntityGID.entityID,
                    out var entityInfo)) //there could be multiple entity descriptors registered in the same group, so it's necessary to check if the entity registered in the group has entityInfoComponent   
            {
#if PARANOID_CHECK
                var hash = new HashSet<IComponentBuilder>(entityInfo.componentsToBuild,
                    default(ComponentBuilderComparer));

                foreach (var component in baseComponents)
                {
                    if (hash.Contains(component) == false)
                        throw new Exception(
                            $"entityInfo.componentsToBuild must contain all the base components {fromEntityGID}," +
                            $" missing component {component}");

                    hash.Remove(component);
                }
#endif
                return entityInfo.componentsToBuild;
            }

            return baseComponents;
        }

        //one datastructure rule them all:
        //split by group
        //split by type per group. It's possible to get all the entities of a give type T per group thanks
        //to the FasterDictionary capabilities OR it's possible to get a specific entityComponent indexed by
        //ID. This ID doesn't need to be the EGID, it can be just the entityID
        //for each group id, save a dictionary indexed by entity type of entities indexed by id
        //                                        group                  EntityComponentType     entityID, EntityComponent
        internal readonly FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>
            _groupEntityComponentsDB;

        //for each entity view type, return the groups (dictionary of entities indexed by entity id) where they are
        //found indexed by group id. TypeSafeDictionary are never created, they instead point to the ones hold
        //by _groupEntityComponentsDB
        //                        <EntityComponentType                            <groupID  <entityID, EntityComponent>>>
        internal readonly FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>>
            _groupsPerEntity;

        //The filters stored for each component and group
        internal readonly FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, LegacyGroupFilters>>
            _groupFilters;

        readonly EntitiesDB _entitiesDB;

        EntitiesDB IUnitTestingInterface.entitiesForTesting => _entitiesDB;
    }

    public interface IUnitTestingInterface
    {
        EntitiesDB entitiesForTesting { get; }
    }
}