using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.Common;
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

        public IEntityFactory GenerateEntityFactory() { return new GenericEntityFactory(this); }
        public IEntityFunctions GenerateEntityFunctions() { return new GenericEntityFunctions(this); }

        ///--------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityInitializer BuildEntity(EGID entityID, IComponentBuilder[] componentsToBuild, Type descriptorType
                                    , IEnumerable<object> implementors = null)
        {
            CheckAddEntityID(entityID, descriptorType);

            DBC.ECS.Check.Require(entityID.groupID.isInvalid == false
                        , "invalid group detected, are you using new ExclusiveGroupStruct() instead of new ExclusiveGroup()?");

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
        void Preallocate(ExclusiveGroupStruct groupID, uint numberOfEntities, IComponentBuilder[] entityComponentsToBuild)
        {
            void PreallocateEntitiesToAdd()
            {
                _groupedEntityToAdd.Preallocate(groupID, numberOfEntities, entityComponentsToBuild);
            }

            void PreallocateDBGroup()
            {
                var numberOfEntityComponents = entityComponentsToBuild.Length;
                FasterDictionary<RefWrapperType, ITypeSafeDictionary> group = GetOrCreateDBGroup(groupID);

                for (var index = 0; index < numberOfEntityComponents; index++)
                {
                    var entityComponentBuilder = entityComponentsToBuild[index];
                    var entityComponentType    = entityComponentBuilder.GetEntityComponentType();

                    var refWrapper = new RefWrapperType(entityComponentType);
                    var dbList         = group.GetOrCreate(refWrapper, ()=>entityComponentBuilder.CreateDictionary(numberOfEntities));
                    entityComponentBuilder.Preallocate(dbList, numberOfEntities);

                    if (_groupsPerEntity.TryGetValue(refWrapper, out var groupedGroup) == false)
                        groupedGroup = _groupsPerEntity[refWrapper] =
                            new FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>();

                    groupedGroup[groupID] = dbList;
                }
            }

            PreallocateDBGroup();
            PreallocateEntitiesToAdd();
            _entityLocator.PreallocateReferenceMaps(groupID, numberOfEntities);
        }

        ///--------------------------------------------
        ///
        void MoveEntityFromAndToEngines(IComponentBuilder[] componentBuilders, EGID fromEntityGID, EGID? toEntityGID)
        {
            using (var sampler = new PlatformProfiler("Move Entity From Engines"))
            {
                var fromGroup = GetDBGroup(fromEntityGID.groupID);

                //Check if there is an EntityInfo linked to this entity, if so it's a DynamicEntityDescriptor!
                if (fromGroup.TryGetValue(new RefWrapperType(ComponentBuilderUtilities.ENTITY_INFO_COMPONENT)
                                        , out var entityInfoDic)
                 && ((ITypeSafeDictionary<EntityInfoComponent>) entityInfoDic).TryGetValue(
                        fromEntityGID.entityID, out var entityInfo))
                    SwapOrRemoveEntityComponents(fromEntityGID, toEntityGID, entityInfo.componentsToBuild, fromGroup
                                               , sampler);
                //otherwise it's a normal static entity descriptor
                else
                    SwapOrRemoveEntityComponents(fromEntityGID, toEntityGID, componentBuilders, fromGroup, sampler);
            }
        }

        void SwapOrRemoveEntityComponents
        (EGID fromEntityGID, EGID? toEntityGID, IComponentBuilder[] entitiesToMove
       , FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup, in PlatformProfiler sampler)
        {
            using (sampler.Sample("MoveEntityComponents"))
            {
                var length = entitiesToMove.Length;

                FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup = null;

                //Swap is not like adding a new entity. While adding new entities happen at the end of submission
                //Adding an entity to a group due to a swap of groups happens now.
                if (toEntityGID.HasValue)
                {
                    var entityGid = toEntityGID.Value;
                    _entityLocator.UpdateEntityReference(fromEntityGID, entityGid);

                    var toGroupID = entityGid.groupID;

                    toGroup = GetOrCreateDBGroup(toGroupID);

                    //Add all the entities to the dictionary
                    for (var i = 0; i < length; i++)
                        CopyEntityToDictionary(fromEntityGID, entityGid, fromGroup, toGroup
                                             , entitiesToMove[i].GetEntityComponentType(), sampler);
                }
                else
                {
                    _entityLocator.RemoveEntityReference(fromEntityGID);
                }

                //call all the callbacks
                for (var i = 0; i < length; i++)
                    ExecuteEnginesSwapOrRemoveCallbacks(fromEntityGID, toEntityGID, fromGroup, toGroup
                                                      , entitiesToMove[i].GetEntityComponentType(), sampler);

                //then remove all the entities from the dictionary
                for (var i = 0; i < length; i++)
                    RemoveEntityFromDictionary(fromEntityGID, fromGroup, entitiesToMove[i].GetEntityComponentType()
                                             , sampler);
            }
        }

        void CopyEntityToDictionary
        (EGID entityGID, EGID toEntityGID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup
       , FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup, Type entityComponentType
       , in PlatformProfiler sampler)
        {
            using (sampler.Sample("CopyEntityToDictionary"))
            {
                var wrapper = new RefWrapperType(entityComponentType);

                ITypeSafeDictionary fromTypeSafeDictionary =
                    GetTypeSafeDictionary(entityGID.groupID, fromGroup, wrapper);

#if DEBUG && !PROFILE_SVELTO
                if (fromTypeSafeDictionary.Has(entityGID.entityID) == false)
                {
                    throw new EntityNotFoundException(entityGID, entityComponentType);
                }
#endif
                ITypeSafeDictionary toEntitiesDictionary =
                    GetOrCreateTypeSafeDictionary(toEntityGID.groupID, toGroup, wrapper, fromTypeSafeDictionary);

                fromTypeSafeDictionary.AddEntityToDictionary(entityGID, toEntityGID, toEntitiesDictionary);
            }
        }

        void ExecuteEnginesSwapOrRemoveCallbacks
        (EGID entityGID, EGID? toEntityGID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup
       , FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup, Type entityComponentType
       , in PlatformProfiler profiler)
        {
            using (profiler.Sample("MoveEntityComponentFromAndToEngines"))
            {
                //add all the entities
                var refWrapper             = new RefWrapperType(entityComponentType);
                var fromTypeSafeDictionary = GetTypeSafeDictionary(entityGID.groupID, fromGroup, refWrapper);

                ITypeSafeDictionary toEntitiesDictionary = null;
                if (toGroup != null)
                    toEntitiesDictionary = toGroup[refWrapper]; //this is guaranteed to exist by AddEntityToDictionary

#if DEBUG && !PROFILE_SVELTO
                if (fromTypeSafeDictionary.Has(entityGID.entityID) == false)
                    throw new EntityNotFoundException(entityGID, entityComponentType);
#endif
                fromTypeSafeDictionary.ExecuteEnginesSwapOrRemoveCallbacks(entityGID, toEntityGID, toEntitiesDictionary
                                                                         , toEntityGID == null
                                                                               ? _reactiveEnginesAddRemove
                                                                               : _reactiveEnginesSwap, in profiler);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RemoveEntityFromDictionary
        (EGID entityGID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup, Type entityComponentType
       , in PlatformProfiler sampler)
        {
            using (sampler.Sample("RemoveEntityFromDictionary"))
            {
                var refWrapper             = new RefWrapperType(entityComponentType);
                var fromTypeSafeDictionary = GetTypeSafeDictionary(entityGID.groupID, fromGroup, refWrapper);

                fromTypeSafeDictionary.RemoveEntityFromDictionary(entityGID);
            }
        }

        /// <summary>
        /// Swap all the entities from one group to another
        ///
        /// TODO: write unit test that also tests that this calls MoveTo callbacks and not Add or Remove.
        /// also that the passing EGID is the same of a component with EGID
        /// </summary>
        /// <param name="fromIdGroupId"></param>
        /// <param name="toGroupId"></param>
        /// <param name="profiler"></param>
        void SwapEntitiesBetweenGroups
            (ExclusiveGroupStruct fromIdGroupId, ExclusiveGroupStruct toGroupId, in PlatformProfiler profiler)
        {
            using (profiler.Sample("SwapEntitiesBetweenGroups"))
            {
                FasterDictionary<RefWrapperType, ITypeSafeDictionary> fromGroup = GetDBGroup(fromIdGroupId);
                FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup   = GetOrCreateDBGroup(toGroupId);

                _entityLocator.UpdateAllGroupReferenceLocators(fromIdGroupId, toGroupId);

                foreach (var dictionaryOfEntities in fromGroup)
                {
                    ITypeSafeDictionary toEntitiesDictionary =
                        GetOrCreateTypeSafeDictionary(toGroupId, toGroup, dictionaryOfEntities.Key
                                                    , dictionaryOfEntities.Value);

                    var groupsOfEntityType = _groupsPerEntity[dictionaryOfEntities.Key];
                    var groupOfEntitiesToCopyAndClear = groupsOfEntityType[fromIdGroupId];

                    toEntitiesDictionary.AddEntitiesFromDictionary(groupOfEntitiesToCopyAndClear, toGroupId, this);

                    //call all the MoveTo callbacks
                    dictionaryOfEntities.Value.ExecuteEnginesAddOrSwapCallbacks(_reactiveEnginesSwap
                      , dictionaryOfEntities.Value, fromIdGroupId, toGroupId, profiler);

                    //todo: if it's unmanaged, I can use fastclear
                    groupOfEntitiesToCopyAndClear.Clear();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        FasterDictionary<RefWrapperType, ITypeSafeDictionary> GetDBGroup(ExclusiveGroupStruct fromIdGroupId)
        {
            if (_groupEntityComponentsDB.TryGetValue(fromIdGroupId
                                                   , out FasterDictionary<RefWrapperType, ITypeSafeDictionary>
                                                         fromGroup) == false)
                throw new ECSException("Group doesn't exist: ".FastConcat(fromIdGroupId.ToName()));

            return fromGroup;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        FasterDictionary<RefWrapperType, ITypeSafeDictionary> GetOrCreateDBGroup
            (ExclusiveGroupStruct toGroupId)
        {
            return _groupEntityComponentsDB.GetOrCreate(
                toGroupId, () => new FasterDictionary<RefWrapperType, ITypeSafeDictionary>());
        }

        ITypeSafeDictionary GetOrCreateTypeSafeDictionary
        (ExclusiveGroupStruct groupId, FasterDictionary<RefWrapperType, ITypeSafeDictionary> toGroup
       , RefWrapperType type, ITypeSafeDictionary fromTypeSafeDictionary)
        {
            //be sure that the TypeSafeDictionary for the entity Type exists
            if (toGroup.TryGetValue(type, out ITypeSafeDictionary toEntitiesDictionary) == false)
            {
                toEntitiesDictionary = fromTypeSafeDictionary.Create();
                toGroup.Add(type, toEntitiesDictionary);
            }

            //update GroupsPerEntity
            if (_groupsPerEntity.TryGetValue(type, out var groupedGroup) == false)
                groupedGroup = _groupsPerEntity[type] =
                    new FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>();

            groupedGroup[groupId] = toEntitiesDictionary;
            return toEntitiesDictionary;
        }

        static ITypeSafeDictionary GetTypeSafeDictionary
            (ExclusiveGroupStruct groupID, FasterDictionary<RefWrapperType, ITypeSafeDictionary> @group, RefWrapperType refWrapper)
        {
            if (@group.TryGetValue(refWrapper, out ITypeSafeDictionary fromTypeSafeDictionary) == false)
            {
                throw new ECSException("no group found: ".FastConcat(groupID.ToName()));
            }

            return fromTypeSafeDictionary;
        }

        void RemoveEntitiesFromGroup(ExclusiveGroupStruct groupID, in PlatformProfiler profiler)
        {
            _entityLocator.RemoveAllGroupReferenceLocators(groupID);

            if (_groupEntityComponentsDB.TryGetValue(groupID, out var dictionariesOfEntities))
            {
                foreach (var dictionaryOfEntities in dictionariesOfEntities)
                {
                    dictionaryOfEntities.Value.ExecuteEnginesRemoveCallbacks(_reactiveEnginesAddRemove, profiler
                                                                           , groupID);
                    dictionaryOfEntities.Value.FastClear();

                    var groupsOfEntityType = _groupsPerEntity[dictionaryOfEntities.Key];
                    groupsOfEntityType[groupID].FastClear();
                }
            }
        }

        //one datastructure rule them all:
        //split by group
        //split by type per group. It's possible to get all the entities of a give type T per group thanks
        //to the FasterDictionary capabilities OR it's possible to get a specific entityComponent indexed by
        //ID. This ID doesn't need to be the EGID, it can be just the entityID
        //for each group id, save a dictionary indexed by entity type of entities indexed by id
        //                         group                EntityComponentType   entityID, EntityComponent
        internal readonly FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>
            _groupEntityComponentsDB;

        //for each entity view type, return the groups (dictionary of entities indexed by entity id) where they are
        //found indexed by group id. TypeSafeDictionary are never created, they instead point to the ones hold
        //by _groupEntityComponentsDB
        //                        <EntityComponentType                            <groupID  <entityID, EntityComponent>>>
        internal readonly FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, ITypeSafeDictionary>>
            _groupsPerEntity;

        //The filters stored for each component and group
        internal readonly FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, GroupFilters>>
            _groupFilters;

        readonly EntitiesDB _entitiesDB;

        EntitiesDB IUnitTestingInterface.entitiesForTesting => _entitiesDB;
    }

    public interface IUnitTestingInterface
    {
        EntitiesDB entitiesForTesting { get; }
    }
}