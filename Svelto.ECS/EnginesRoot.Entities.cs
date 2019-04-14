using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot : IDisposable
    {
        /// <summary>
        /// Dispose an EngineRoot once not used anymore, so that all the
        /// engines are notified with the entities removed.
        /// It's a clean up process.
        /// </summary>
        public void Dispose()
        {
            var profiler = new PlatformProfiler();
            using (profiler.StartNewSession("Final Dispose"))
            {
                foreach (var groups in _groupEntityDB)
                    foreach (var entityList in groups.Value)
                    {
                        try
                        {
                            entityList.Value.RemoveEntitiesFromEngines(_reactiveEnginesAddRemove, ref profiler);
                        }
                        catch (Exception e)
                        {
                            Console.LogException(e);
                        }
                    }

                foreach (var engine in _disposableEngines)
                    try
                    {
                        engine.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.LogException(e);
                    }
            }

            GC.SuppressFinalize(this);
        }

        ~EnginesRoot()
        {
            Console.LogWarning("Engines Root has been garbage collected, don't forget to call Dispose()!");

            Dispose();
        }

        ///--------------------------------------------
        ///
        public IEntityStreamConsumerFactory GenerateConsumerFactory()
        {
            return new GenericentityStreamConsumerFactory(new DataStructures.WeakReference<EnginesRoot>(this));
        }

        public IEntityFactory GenerateEntityFactory()
        {
            return new GenericEntityFactory(new DataStructures.WeakReference<EnginesRoot>(this));
        }

        public IEntityFunctions GenerateEntityFunctions()
        {
            return new GenericEntityFunctions(new DataStructures.WeakReference<EnginesRoot>(this));
        }

        ///--------------------------------------------
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityStructInitializer BuildEntity<T>(EGID entityID, object[] implementors) where T : IEntityDescriptor, new()
        {
            return BuildEntity(entityID, EntityDescriptorTemplate<T>.descriptor, implementors);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityStructInitializer BuildEntity<T>(EGID entityID, T entityDescriptor, object[] implementors)
            where T : IEntityDescriptor
        {
            var descriptorEntitiesToBuild = entityDescriptor.entitiesToBuild;

            CheckAddEntityID(entityID, entityDescriptor);

            var dic = EntityFactory.BuildGroupedEntities(entityID, _groupedEntityToAdd,
                                                         descriptorEntitiesToBuild, implementors);

            return new EntityStructInitializer(entityID, dic);
        }

#if REAL_ID
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        EntityStructInitializer BuildEntity<T>(ExclusiveGroup.ExclusiveGroupStruct groupID, object[] implementors) where T : IEntityDescriptor, new()
        {
            //temporary egid, will change during the real submission, needed for the initialization look up
            var egid = EGID.CREATE_WITHOUT_ID(groupID, _groupedEntityToAdd.entitiesBuiltThisSubmission++);
            
            var dic = EntityFactory.BuildGroupedEntities(egid, _groupedEntityToAdd.current,
                                                         EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, implementors);
            
            return new EntityStructInitializer(egid, dic.groups);
        }
#endif

        ///--------------------------------------------
        void Preallocate<T>(uint groupID, uint size) where T : IEntityDescriptor, new()
        {
            var entityViewsToBuild = EntityDescriptorTemplate<T>.descriptor.entitiesToBuild;
            var numberOfEntityViews              = entityViewsToBuild.Length;
            
            //reserve space in the database
            if (_groupEntityDB.TryGetValue(groupID, out var @group) == false)
                group = _groupEntityDB[groupID] = new Dictionary<Type, ITypeSafeDictionary>();

            for (var index = 0; index < numberOfEntityViews; index++)
            {
                var entityViewBuilder = entityViewsToBuild[index];
                var entityViewType = entityViewBuilder.GetEntityType();

                if (group.TryGetValue(entityViewType, out var dbList) == false)
                    group[entityViewType] = entityViewBuilder.Preallocate(ref dbList, size);
                else
                    dbList.SetCapacity(size);

                if (_groupsPerEntity.TryGetValue(entityViewType, out var groupedGroup) == false)
                    groupedGroup = _groupsPerEntity[entityViewType] = new FasterDictionary<uint, ITypeSafeDictionary>();

                groupedGroup[groupID] = dbList;
            }
        }

        ///--------------------------------------------
        /// 
        void MoveEntity(IEntityBuilder[] entityBuilders, EGID fromEntityGID, Type originalDescriptorType, EGID? toEntityGID)
        {
            var profiler = new PlatformProfiler();
            using (profiler.StartNewSession("Move Entity"))
            {
                //for each entity view generated by the entity descriptor
                if (_groupEntityDB.TryGetValue(fromEntityGID.groupID, out var fromGroup) == false)
                    throw new ECSException("from group not found eid: ".FastConcat(fromEntityGID.entityID)
                                              .FastConcat(" group: ").FastConcat(fromEntityGID.groupID));

                //Check if there is an EntityInfoView linked to this entity, if so it's a DynamicEntityDescriptor!
                bool correctEntityDescriptorFound = true;

                EntityStructInfoView entityInfoView = default;
                if (fromGroup.TryGetValue(ENTITY_INFO_VIEW_TYPE, out var entityInfoViewDic) &&
                    (entityInfoViewDic as TypeSafeDictionary<EntityStructInfoView>).TryGetValue(
                        fromEntityGID.entityID, out entityInfoView) && (correctEntityDescriptorFound =
                        entityInfoView.type == originalDescriptorType))
                {
                    var entitiesToMove = entityInfoView.entitiesToBuild;

                    Dictionary<Type, ITypeSafeDictionary> toGroup = null;

                    if (toEntityGID != null)
                    {
                        var toGroupID = toEntityGID.Value.groupID;
                        
                        if (_groupEntityDB.TryGetValue(toGroupID, out toGroup) == false)
                            toGroup = _groupEntityDB[toGroupID] = new Dictionary<Type, ITypeSafeDictionary>();
                    }

                    for (int i = 0; i < entitiesToMove.Length; i++)
                        MoveEntityView(fromEntityGID, toEntityGID, toGroup, ref fromGroup,
                                       entitiesToMove[i].GetEntityType(), profiler);
                }
                //otherwise it's a normal static entity descriptor
                else
                {
#if DEBUG && !PROFILER
                    if (correctEntityDescriptorFound == false)
                    throw new ECSException(INVALID_DYNAMIC_DESCRIPTOR_ERROR.FastConcat(" ID ").FastConcat(fromEntityGID.entityID)
                                               .FastConcat(" group ID ").FastConcat(fromEntityGID.groupID).FastConcat(
                                                " descriptor found: ", entityInfoView.type.Name, " descriptor Excepted ",
                                                originalDescriptorType.Name));
#endif

                    Dictionary<Type, ITypeSafeDictionary> toGroup = null;
                    if (toEntityGID != null)
                    {
                        var toGroupID = toEntityGID.Value.groupID;
                        
                        if (_groupEntityDB.TryGetValue(toGroupID, out toGroup) == false)
                            toGroup = _groupEntityDB[toGroupID] = new Dictionary<Type, ITypeSafeDictionary>();
                    }

                    for (var i = 0; i < entityBuilders.Length; i++)
                        MoveEntityView(fromEntityGID, toEntityGID, toGroup, ref fromGroup,
                                       entityBuilders[i].GetEntityType(), profiler);
                }
            }
        }

        void MoveEntityView(EGID entityGID, EGID? toEntityGID, Dictionary<Type, ITypeSafeDictionary> toGroup,
            ref Dictionary<Type, ITypeSafeDictionary> fromGroup, Type entityViewType, PlatformProfiler profiler)
        {
            if (fromGroup.TryGetValue(entityViewType, out var fromTypeSafeDictionary) == false)
            {
                throw new ECSException("no entities in from group eid: ".FastConcat(entityGID.entityID)
                                          .FastConcat(" group: ").FastConcat(entityGID.groupID));
            }

            ITypeSafeDictionary toEntitiesDictionary = null;

            //in case we want to move to a new group, otherwise is just a remove
            if (toEntityGID != null)
            {
                if (toGroup.TryGetValue(entityViewType, out toEntitiesDictionary) == false)
                {
                    toEntitiesDictionary = fromTypeSafeDictionary.Create();
                    toGroup.Add(entityViewType, toEntitiesDictionary);
                }

                if (_groupsPerEntity.TryGetValue(entityViewType, out var groupedGroup) == false)
                    groupedGroup = _groupsPerEntity[entityViewType] = new FasterDictionary<uint, ITypeSafeDictionary>();

                groupedGroup[toEntityGID.Value.groupID] = toEntitiesDictionary;
            }

            if (fromTypeSafeDictionary.Has(entityGID.entityID) == false)
            {
                throw new EntityNotFoundException(entityGID, entityViewType);
            }

            fromTypeSafeDictionary.MoveEntityFromDictionaryAndEngines(entityGID, toEntityGID,
                                                                      toEntitiesDictionary,
                                                                      toEntityGID == null ? _reactiveEnginesAddRemove :
                                                                      _reactiveEnginesSwap,
                                                                      ref profiler);

            if (fromTypeSafeDictionary.Count == 0) //clean up
            {
                _groupsPerEntity[entityViewType].Remove(entityGID.groupID);

                //I don't remove the group if empty on purpose, in case it needs to be reused however I trim it to save
                //memory
                fromTypeSafeDictionary.Trim();
            }
        }

        void RemoveGroupAndEntitiesFromDB(uint groupID, Type entityDescriptor)
        {
            throw new NotImplementedException();
            /*  var profiler = new PlatformProfiler();
              using (profiler.StartNewSession("Remove Group Of Entities"))
              {
                  FasterDictionary<int, ITypeSafeDictionary> @group;
                  if (_groupsPerEntity.TryGetValue(entityDescriptor, out group))
                  {
                      if (group.TryGetValue())
                      foreach (var entity in group)
                      {
                          MoveEntity(entity.);
                      }                    
                  }
              }*/
        }

        void RemoveGroupAndEntitiesFromDB(uint groupID)
        {
            var profiler = new PlatformProfiler();
            using (profiler.StartNewSession("Remove Group"))
            {
                var dictionariesOfEntities = _groupEntityDB[groupID];
                foreach (var dictionaryOfEntities in dictionariesOfEntities)
                {
                    var platformProfiler = profiler;
                    dictionaryOfEntities.Value.RemoveEntitiesFromEngines(_reactiveEnginesAddRemove, ref platformProfiler);
                    var groupedGroupOfEntities = _groupsPerEntity[dictionaryOfEntities.Key];
                    groupedGroupOfEntities.Remove(groupID);
                }

                //careful, in this case I assume you really don't want to use this group anymore
                //so I remove it from the database
                _groupEntityDB.Remove(groupID);
            }
        }

        ///--------------------------------------------
        void SwapEntityGroup(IEntityBuilder[] builders, Type originalEntityDescriptor, EGID fromEntityID,
            EGID toEntityID)
        {
            DBC.ECS.Check.Require(fromEntityID.groupID != toEntityID.groupID,
                                  "entity group is the same of the destination group");

            MoveEntity(builders, fromEntityID, originalEntityDescriptor, toEntityID);
        }

        internal Consumer<T> GenerateConsumer<T>(string name, int capacity) where T : unmanaged, IEntityStruct
        {
            return _entitiesStream.GenerateConsumer<T>(name, capacity);
        }

        const string INVALID_DYNAMIC_DESCRIPTOR_ERROR =
            "Found an entity requesting an invalid dynamic descriptor, this " +
            "can happen only if you are building different entities with the " +
            "same ID in the same group! The operation will continue using" + "the base descriptor only ";
    }
}