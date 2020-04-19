using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS
{
    public partial class EnginesRoot : IDisposable
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
        EntityComponentInitializer BuildEntity(EGID entityID, IComponentBuilder[] componentsToBuild,
            IEnumerable<object> implementors = null)
        {
            CheckAddEntityID(entityID);

            var dic = EntityFactory.BuildGroupedEntities(entityID, _groupedEntityToAdd, componentsToBuild, implementors);

            return new EntityComponentInitializer(entityID, dic);
        }

        ///--------------------------------------------
        void Preallocate<T>(ExclusiveGroupStruct groupID, uint size) where T : IEntityDescriptor, new()
        {
            var entityComponentsToBuild = EntityDescriptorTemplate<T>.descriptor.componentsToBuild;
            var numberOfEntityComponents = entityComponentsToBuild.Length;

            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> group = GetOrCreateGroup(groupID);

            for (var index = 0; index < numberOfEntityComponents; index++)
            {
                var entityComponentBuilder = entityComponentsToBuild[index];
                var entityComponentType = entityComponentBuilder.GetEntityComponentType();

                var refWrapper = new RefWrapper<Type>(entityComponentType);
                if (group.TryGetValue(refWrapper, out var dbList) == false)
                    group[refWrapper] = entityComponentBuilder.Preallocate(ref dbList, size);
                else
                    dbList.SetCapacity(size);

                if (_groupsPerEntity.TryGetValue(refWrapper, out var groupedGroup) == false)
                    groupedGroup = _groupsPerEntity[refWrapper] = new FasterDictionary<uint, ITypeSafeDictionary>();

                groupedGroup[groupID] = dbList;
            }
        }

        ///--------------------------------------------
        ///
        void MoveEntityFromAndToEngines(IComponentBuilder[] entityBuilders, EGID fromEntityGID, EGID? toEntityGID)
        {
            using (var sampler = new PlatformProfiler("Move Entity From Engines"))
            {
                var fromGroup = GetGroup(fromEntityGID.groupID);

                //Check if there is an EntityInfoView linked to this entity, if so it's a DynamicEntityDescriptor!
                if (fromGroup.TryGetValue(new RefWrapper<Type>(EntityBuilderUtilities.ENTITY_STRUCT_INFO_VIEW),
                        out var entityInfoViewDic) &&
                    (entityInfoViewDic as ITypeSafeDictionary<EntityInfoComponentView>).TryGetValue(fromEntityGID.entityID,
                        out var entityInfoView))
                    MoveEntityComponents(fromEntityGID, toEntityGID, entityInfoView.componentsToBuild, fromGroup, sampler);
                //otherwise it's a normal static entity descriptor
                else
                    MoveEntityComponents(fromEntityGID, toEntityGID, entityBuilders, fromGroup, sampler);
            }
        }

        void MoveEntityComponents(EGID fromEntityGID, EGID? toEntityGID, IComponentBuilder[] entitiesToMove,
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> fromGroup,
            PlatformProfiler sampler)
        {
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> toGroup = null;

            if (toEntityGID != null)
            {
                var toGroupID = toEntityGID.Value.groupID;

                toGroup = GetOrCreateGroup(toGroupID);

                //Add all the entities to the dictionary
                for (var i = 0; i < entitiesToMove.Length; i++)
                    CopyEntityToDictionary(fromEntityGID, toEntityGID.Value, fromGroup, toGroup,
                        entitiesToMove[i].GetEntityComponentType());
            }

            //call all the callbacks
            for (var i = 0; i < entitiesToMove.Length; i++)
                MoveEntityComponentFromAndToEngines(fromEntityGID, toEntityGID, fromGroup, toGroup,
                    entitiesToMove[i].GetEntityComponentType(), sampler);

            //then remove all the entities from the dictionary
            for (var i = 0; i < entitiesToMove.Length; i++)
                RemoveEntityFromDictionary(fromEntityGID, fromGroup, entitiesToMove[i].GetEntityComponentType(), sampler);
        }

        void CopyEntityToDictionary(EGID entityGID, EGID toEntityGID,
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> fromGroup,
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> toGroup,
            Type entityComponentType)
        {
            var wrapper = new RefWrapper<Type>(entityComponentType);

            ITypeSafeDictionary fromTypeSafeDictionary = GetTypeSafeDictionary(entityGID.groupID, fromGroup, wrapper);

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

        void MoveEntityComponentFromAndToEngines(EGID entityGID, EGID? toEntityGID,
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> fromGroup,
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> toGroup,
            Type entityComponentType, in PlatformProfiler profiler)
        {
            //add all the entities
            var refWrapper = new RefWrapper<Type>(entityComponentType);
            var fromTypeSafeDictionary = GetTypeSafeDictionary(entityGID.groupID, fromGroup, refWrapper);

            ITypeSafeDictionary toEntitiesDictionary = null;
            if (toGroup != null)
                toEntitiesDictionary = toGroup[refWrapper]; //this is guaranteed to exist by AddEntityToDictionary

#if DEBUG && !PROFILE_SVELTO
            if (fromTypeSafeDictionary.Has(entityGID.entityID) == false)
                throw new EntityNotFoundException(entityGID, entityComponentType);
#endif
            fromTypeSafeDictionary.MoveEntityFromEngines(entityGID, toEntityGID, toEntitiesDictionary,
                toEntityGID == null ? _reactiveEnginesAddRemove : _reactiveEnginesSwap, in profiler);
        }

        void RemoveEntityFromDictionary(EGID entityGID,
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> fromGroup,
            Type entityComponentType, in PlatformProfiler profiler)
        {
            var refWrapper = new RefWrapper<Type>(entityComponentType);
            var fromTypeSafeDictionary = GetTypeSafeDictionary(entityGID.groupID, fromGroup, refWrapper);

            fromTypeSafeDictionary.RemoveEntityFromDictionary(entityGID);

            //if (fromTypeSafeDictionary.Count == 0) //clean up
            {
                //todo: this must be unit tested properly
                //_groupsPerEntity[refWrapper].Remove(entityGID.groupID);
                //I don't remove the group if empty on purpose, in case it needs to be reused
            }
        }

        /// <summary>
        /// Swap all the entities from one group to another
        /// </summary>
        /// <param name="fromIdGroupId"></param>
        /// <param name="toGroupId"></param>
        /// <param name="profiler"></param>
        void SwapEntitiesBetweenGroups(uint fromIdGroupId, uint toGroupId, in PlatformProfiler profiler)
        {
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> fromGroup = GetGroup(fromIdGroupId);
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> toGroup = GetOrCreateGroup(toGroupId);

            foreach (FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>.KeyValuePairFast dictionaryOfEntities in
                fromGroup)
            {
                //call all the MoveTo callbacks
                dictionaryOfEntities.Value.AddEntitiesToEngines(_reactiveEnginesAddRemove, dictionaryOfEntities.Value,
                    new ExclusiveGroupStruct(toGroupId), profiler);

                ITypeSafeDictionary toEntitiesDictionary = GetOrCreateTypeSafeDictionary(toGroupId, toGroup, 
                    dictionaryOfEntities.Key, dictionaryOfEntities.Value);

                FasterDictionary<uint, ITypeSafeDictionary> groupsOfEntityType =
                    _groupsPerEntity[dictionaryOfEntities.Key];

                ITypeSafeDictionary typeSafeDictionary = groupsOfEntityType[fromIdGroupId];
                toEntitiesDictionary.AddEntitiesFromDictionary(typeSafeDictionary, toGroupId);

                typeSafeDictionary.FastClear();
            }
        }

        FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> GetGroup(uint fromIdGroupId)
        {
            if (_groupEntityComponentsDB.TryGetValue(fromIdGroupId,
                    out FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> fromGroup) == false)
                throw new ECSException("Group doesn't exist: ".FastConcat(fromIdGroupId));
            return fromGroup;
        }

        FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> GetOrCreateGroup(uint toGroupId)
        {
            if (_groupEntityComponentsDB.TryGetValue(toGroupId,
                    out FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> toGroup) == false)
                toGroup = _groupEntityComponentsDB[toGroupId] =
                    new FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>();
            
            return toGroup;
        }

        ITypeSafeDictionary GetOrCreateTypeSafeDictionary(uint groupId,
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> toGroup,
            RefWrapper<Type> type, ITypeSafeDictionary fromTypeSafeDictionary)
        {
            //be sure that the TypeSafeDictionary for the entity Type exists
            if (toGroup.TryGetValue(type, out ITypeSafeDictionary toEntitiesDictionary) ==
                false)
            {
                toEntitiesDictionary = fromTypeSafeDictionary.Create();
                toGroup.Add(type, toEntitiesDictionary);
            }

            //update GroupsPerEntity
            if (_groupsPerEntity.TryGetValue(type, out var groupedGroup) == false)
                groupedGroup = _groupsPerEntity[type] =
                    new FasterDictionary<uint, ITypeSafeDictionary>();

            groupedGroup[groupId] = toEntitiesDictionary;
            return toEntitiesDictionary;
        }

        static ITypeSafeDictionary GetTypeSafeDictionary(uint groupID,
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> @group, RefWrapper<Type> refWrapper)
        {
            if (@group.TryGetValue(refWrapper, out ITypeSafeDictionary fromTypeSafeDictionary) == false)
            {
                throw new ECSException("no group found: ".FastConcat(groupID));
            }

            return fromTypeSafeDictionary;
        }

        void RemoveGroupAndEntities(uint groupID, in PlatformProfiler profiler)
        {
            FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> dictionariesOfEntities =
                _groupEntityComponentsDB[groupID];

            foreach (var dictionaryOfEntities in dictionariesOfEntities)
            {
                dictionaryOfEntities.Value.RemoveEntitiesFromEngines(_reactiveEnginesAddRemove, profiler,
                    new ExclusiveGroupStruct(groupID));
                dictionaryOfEntities.Value.FastClear();

                FasterDictionary<uint, ITypeSafeDictionary> groupsOfEntityType =
                    _groupsPerEntity[dictionaryOfEntities.Key];
                groupsOfEntityType[groupID].FastClear();
            }
        }

        internal Consumer<T> GenerateConsumer<T>(string name, uint capacity) where T : unmanaged, IEntityComponent
        {
            return _entitiesStream.GenerateConsumer<T>(name, capacity);
        }

        internal Consumer<T> GenerateConsumer<T>(ExclusiveGroup group, string name, uint capacity)
            where T : unmanaged, IEntityComponent
        {
            return _entitiesStream.GenerateConsumer<T>(group, name, capacity);
        }

        //one datastructure rule them all:
        //split by group
        //split by type per group. It's possible to get all the entities of a give type T per group thanks
        //to the FasterDictionary capabilities OR it's possible to get a specific entityComponent indexed by
        //ID. This ID doesn't need to be the EGID, it can be just the entityID
        //for each group id, save a dictionary indexed by entity type of entities indexed by id
        //                         group                EntityComponentType   entityID, EntityComponent
        readonly FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> _groupEntityComponentsDB;

        //for each entity view type, return the groups (dictionary of entities indexed by entity id) where they are
        //found indexed by group id. TypeSafeDictionary are never created, they instead point to the ones hold
        //by _groupEntityComponentsDB
        //                        EntityComponentType                    groupID  entityID, EntityComponent
        readonly FasterDictionary<RefWrapper<Type>, FasterDictionary<uint, ITypeSafeDictionary>> _groupsPerEntity;

        readonly EntitiesDB     _entitiesDB;
        readonly EntitiesStream _entitiesStream;
    }
}