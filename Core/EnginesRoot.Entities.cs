//#define PARANOID_CHECK

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public static class EGIDMultiMapperNBExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EGIDMultiMapper<T> QueryMappedEntities<T>(this EntitiesDB entitiesDb, LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
                where T : struct, _IInternalEntityComponent
        {
            var dictionary = new FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary<T>>((uint) groups.count);
        
            foreach (var group in groups)
            {
                entitiesDb.QueryOrCreateEntityDictionary<T>(group, out var typeSafeDictionary);
                        //if (typeSafeDictionary.count > 0) avoiding this allows these egidmappers to be precreated and stored
                dictionary.Add(group, typeSafeDictionary as ITypeSafeDictionary<T>);
            }
            
            return new EGIDMultiMapper<T>(dictionary);
        }
    }

    public partial class EnginesRoot: IDisposable, IUnitTestingInterface
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

            DBC.ECS.Check.Require(
                entityID.groupID.isInvalid == false,
                "invalid group detected, are you using new ExclusiveGroupStruct() instead of new ExclusiveGroup()?");

            var reference = _entityLocator.ClaimReference();
            _entityLocator.SetReference(reference, entityID);

            var dic = EntityFactory.BuildGroupedEntities(
                entityID, _groupedEntityToAdd, componentsToBuild, implementors
#if DEBUG && !PROFILE_SVELTO
              , descriptorType
#endif
            );

            return new EntityInitializer(entityID, dic, reference);
        }

        /// <summary>
        /// Preallocate memory to avoid the impact to resize arrays when many entities are submitted at once
        /// </summary>
        internal void Preallocate(ExclusiveGroupStruct groupID, uint size, IComponentBuilder[] entityComponentsToBuild)
        {
            void PreallocateEntitiesToAdd()
            {
                _groupedEntityToAdd.Preallocate(groupID, size, entityComponentsToBuild);
            }

            void PreallocateDBGroup()
            {
                var numberOfEntityComponents = entityComponentsToBuild.Length;
                FasterDictionary<ComponentID, ITypeSafeDictionary> group = GetOrAddDBGroup(groupID);

                for (var index = 0; index < numberOfEntityComponents; index++)
                {
                    var entityComponentBuilder = entityComponentsToBuild[index];
                    var entityComponentType = entityComponentBuilder.getComponentID;

                    var components = group.GetOrAdd(entityComponentType, () => entityComponentBuilder.CreateDictionary(size));
                    if (components.count != 0)
                        throw new ECSException("Entity already created in this group, cannot preallocate");
                    entityComponentBuilder.Preallocate(components, size);

                    if (_groupsPerEntity.TryGetValue(entityComponentType, out var groupedGroup) == false)
                        groupedGroup = _groupsPerEntity[entityComponentType] =
                                new FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>();

                    groupedGroup[groupID] = components;
                }
            }

            PreallocateDBGroup();
            PreallocateEntitiesToAdd();
            _entityLocator.PreallocateReferenceMaps(groupID, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        FasterDictionary<ComponentID, ITypeSafeDictionary> GetDBGroup(ExclusiveGroupStruct fromIdGroupId)
        {
            if (_groupEntityComponentsDB.TryGetValue(
                    fromIdGroupId,
                    out FasterDictionary<ComponentID, ITypeSafeDictionary> fromGroup) == false)
                throw new ECSException("Group doesn't exist: ".FastConcat(fromIdGroupId.ToName()));

            return fromGroup;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        FasterDictionary<ComponentID, ITypeSafeDictionary> GetOrAddDBGroup(ExclusiveGroupStruct toGroupId)
        {
            return _groupEntityComponentsDB.GetOrAdd(
                toGroupId,
                () => new FasterDictionary<ComponentID, ITypeSafeDictionary>());
        }

        IComponentBuilder[] FindRealComponents<T>(EGID fromEntityGID) where T : IEntityDescriptor, new()
        {
            var fromGroup = GetDBGroup(fromEntityGID.groupID);

            if (fromGroup.TryGetValue(
                    ComponentBuilderUtilities.ENTITY_INFO_COMPONENT_ID,
                    out var entityInfoDic) //<entity ID, EntityInfoComponent>
             && ((ITypeSafeDictionary<EntityInfoComponent>)entityInfoDic).TryGetValue(
                    fromEntityGID.entityID,
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

            return EntityDescriptorTemplate<T>.realDescriptor.componentsToBuild;
        }

        IComponentBuilder[] FindRealComponents(EGID fromEntityGID, IComponentBuilder[] baseComponents)
        {
            var fromGroup = GetDBGroup(fromEntityGID.groupID);

            if (fromGroup.TryGetValue(
                    ComponentBuilderUtilities.ENTITY_INFO_COMPONENT_ID,
                    out var entityInfoDic) //<entity ID, EntityInfoComponent>
             && ((ITypeSafeDictionary<EntityInfoComponent>)entityInfoDic).TryGetValue(
                    fromEntityGID.entityID,
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
        internal readonly FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>
                _groupEntityComponentsDB;

        //for each entity view type, return the groups (dictionary of entities indexed by entity id) where they are
        //found indexed by group id. TypeSafeDictionary are never created, they instead point to the ones hold
        //by _groupEntityComponentsDB
        //                        <EntityComponentType                            <groupID  <entityID, EntityComponent>>>
        internal readonly FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>>
                _groupsPerEntity;

        readonly EntitiesDB _entitiesDB;

        EntitiesDB IUnitTestingInterface.entitiesForTesting => _entitiesDB;
    }

    public interface IUnitTestingInterface
    {
        EntitiesDB entitiesForTesting { get; }
    }
}