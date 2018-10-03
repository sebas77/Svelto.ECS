﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

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
            foreach (var groups in _groupEntityDB)
                foreach (var entityList in groups.Value)
                    entityList.Value.RemoveEntitiesFromEngines(_entityEngines);
            
            foreach (var engine in _disposableEngines)
                engine.Dispose();
        }

        ///--------------------------------------------

        public IEntityFactory GenerateEntityFactory()
        {
            return new GenericEntityFactory(new DataStructures.WeakReference<EnginesRoot>(this));
        }

        public IEntityFunctions GenerateEntityFunctions()
        {
            return new GenericEntityFunctions(new DataStructures.WeakReference<EnginesRoot>(this));
        }

        ///--------------------------------------------

        EntityStructInitializer BuildEntity<T>(EGID entityID, object[] implementors)
            where T : IEntityDescriptor, new()
        {
            return BuildEntity(entityID, EntityDescriptorTemplate<T>.descriptor, implementors);
        }

        EntityStructInitializer BuildEntity<T>(EGID entityID, 
                                T entityDescriptor,
                                object[] implementors) where T:IEntityDescriptor
        {
            var descriptorEntitiesToBuild = entityDescriptor.entitiesToBuild;
            
#if DEBUG && !PROFILER            
            CheckEntityID(entityID, entityDescriptor);
#endif            
            var dic = EntityFactory.BuildGroupedEntities(entityID,
                                                  _groupedEntityToAdd.current,
                                                            descriptorEntitiesToBuild,
                                                   implementors);
            
            _newEntitiesBuiltToProcess++;
            
            return new EntityStructInitializer(entityID, dic);
        }
        
        void CheckEntityID(EGID entityID, IEntityDescriptor descriptorEntity)
        {
            Dictionary<Type, ITypeSafeDictionary> @group;
            var descriptorEntitiesToBuild = descriptorEntity.entitiesToBuild;
            
            if (_groupEntityDB.TryGetValue(entityID.groupID, out @group) == true)
            {
                for (int i = 0; i < descriptorEntitiesToBuild.Length; i++)
                {
                    CheckEntityID(entityID, descriptorEntitiesToBuild[i].GetEntityType(), @group, descriptorEntity.ToString());
                }
            }
        }

        static void CheckEntityID(EGID entityID, Type entityType, Dictionary<Type, ITypeSafeDictionary> @group, string name)
        {
            ITypeSafeDictionary entities;
            if (@group.TryGetValue(entityType, out entities))
            {
                if (entities.Has(entityID.entityID) == true)
                {
                    Utility.Console.LogError("Entity ".FastConcat(name, " with used ID is about to be built: ")
                                            .FastConcat(entityType)
                                            .FastConcat(" id: ")
                                            .FastConcat(entityID.entityID)
                                            .FastConcat(" groupid: ")
                                            .FastConcat(entityID.groupID));
                }
            }
        }

        ///--------------------------------------------

        void Preallocate<T>(int groupID, int size) where T : IEntityDescriptor, new()
        {
            var entityViewsToBuild = EntityDescriptorTemplate<T>.descriptor.entitiesToBuild;
            var count              = entityViewsToBuild.Length;

            //reserve space in the database
            Dictionary<Type, ITypeSafeDictionary> @group;
            if (_groupEntityDB.TryGetValue(groupID, out group) == false)
                group = _groupEntityDB[groupID] = new Dictionary<Type, ITypeSafeDictionary>();

            //reserve space in building buffer
            Dictionary<Type, ITypeSafeDictionary> @groupBuffer;
            if (_groupedEntityToAdd.current.TryGetValue(groupID, out @groupBuffer) == false)
                @groupBuffer = _groupedEntityToAdd.current[groupID] = new Dictionary<Type, ITypeSafeDictionary>();

            ITypeSafeDictionary dbList;

            for (var index = 0; index < count; index++)
            {
                var entityViewBuilder = entityViewsToBuild[index];
                var entityViewType    = entityViewBuilder.GetEntityType();

                if (group.TryGetValue(entityViewType, out dbList) == false)
                    group[entityViewType] = entityViewBuilder.Preallocate(ref dbList, size);
                else
                    dbList.AddCapacity(size);
                
                if (@groupBuffer.TryGetValue(entityViewType, out dbList) == false)
                    @groupBuffer[entityViewType] = entityViewBuilder.Preallocate(ref dbList, size);
                else
                    dbList.AddCapacity(size);
            }
        }
        
        ///--------------------------------------------
        /// 
        void MoveEntity(IEntityBuilder[] entityBuilders, EGID entityGID, int toGroupID = -1,
                        Dictionary<Type, ITypeSafeDictionary> toGroup = null)
        {
            var entityBuildersCount = entityBuilders.Length;
           
            //for each entity view generated by the entity descriptor
            DBC.ECS.Check.Require(_groupEntityDB.ContainsKey(entityGID.groupID) == true, "from group not found");
            var fromGroup = _groupEntityDB[entityGID.groupID];
            
            ITypeSafeDictionary entityInfoViewDic;
            if (fromGroup.TryGetValue(_entityInfoView, out entityInfoViewDic) == true)
            {
                var realEntityInfoView = entityInfoViewDic as TypeSafeDictionary<EntityInfoView>;
                var entitiesToMove = realEntityInfoView[entityGID.entityID].entitiesToBuild;
                
                for (int i = 0; i < entitiesToMove.Length; i++)
                    MoveEntityView(entityGID, toGroupID, toGroup, fromGroup, entitiesToMove[i].GetEntityType());
            }
            else
            {
                for (var i = 0; i < entityBuildersCount; i++)
                {
                    var entityType = entityBuilders[i].GetEntityType();

                    MoveEntityView(entityGID, toGroupID, toGroup, fromGroup, entityType);
                }
            }
        }

        void MoveEntityView(EGID entityGID, int toGroupID, Dictionary<Type, ITypeSafeDictionary> toGroup, 
                            Dictionary<Type, ITypeSafeDictionary> fromGroup, Type entityType)
        {
            DBC.ECS.Check.Require(fromGroup.ContainsKey(entityType) == true, "from group not found");
            var                 fromTypeSafeDictionary = fromGroup[entityType];
            ITypeSafeDictionary dictionaryOfEntities         = null;

            //in case we want to move to a new group, otherwise is just a remove
            if (toGroup != null)
            {
                if (toGroup.TryGetValue(entityType, out dictionaryOfEntities) == false)
                {
                    dictionaryOfEntities = fromTypeSafeDictionary.Create();
                    toGroup.Add(entityType, dictionaryOfEntities);
                }

                FasterDictionary<int, ITypeSafeDictionary> groupedGroup;
                if (_groupedGroups.TryGetValue(entityType, out groupedGroup) == false)
                    groupedGroup = _groupedGroups[entityType] = new FasterDictionary<int, ITypeSafeDictionary>();
                
                groupedGroup[toGroupID] = dictionaryOfEntities;
            }

            DBC.ECS.Check.Assert(fromTypeSafeDictionary.Has(entityGID.entityID), "entity not found");
            fromTypeSafeDictionary.MoveEntityFromDictionaryAndEngines(entityGID, toGroupID, dictionaryOfEntities, _entityEngines);

            if (fromTypeSafeDictionary.Count == 0) //clean up
            {
                _groupedGroups[entityType].Remove(entityGID.groupID);

                //I don't remove the group if empty on purpose, in case it needs to be reused
                //however I trim it to save memory
                fromTypeSafeDictionary.Trim();
            }
        }

        void RemoveGroupAndEntitiesFromDB(int groupID)
        {
            var dictionariesOfEntities = _groupEntityDB[groupID];
            foreach (var dictionaryOfEntities in dictionariesOfEntities)
            {
                dictionaryOfEntities.Value.RemoveEntitiesFromEngines(_entityEngines);
                var groupedGroupOfEntities = _groupedGroups[dictionaryOfEntities.Key];
                groupedGroupOfEntities.Remove(groupID);
            }

            //careful, in this case I assume you really don't want to use this group anymore
            //so I remove it from the database
            _groupEntityDB.Remove(groupID);
        }

        ///--------------------------------------------

        void SwapEntityGroup(IEntityBuilder[] builders, int entityID, int fromGroupID, int toGroupID)
        {
            DBC.ECS.Check.Require(fromGroupID != toGroupID, "the entity is already in this group");

            Dictionary<Type, ITypeSafeDictionary> toGroup;

            if (_groupEntityDB.TryGetValue(toGroupID, out toGroup) == false)
                toGroup = _groupEntityDB[toGroupID] = new Dictionary<Type, ITypeSafeDictionary>();

            MoveEntity(builders, new EGID(entityID, fromGroupID), toGroupID, toGroup);
        }
        
        readonly EntitiesDB         _DB;
        int                         _newEntitiesBuiltToProcess;
        Type                        _entityInfoView = typeof(EntityInfoView);
    }

    public struct EntityStructInitializer
    {
        public EntityStructInitializer(EGID id, Dictionary<Type, ITypeSafeDictionary> current)
        {
            _current = current;
            _id = id;
        }

        public void Init<T>(T initializer) where T: struct, IEntityStruct
        {
            var typeSafeDictionary = (TypeSafeDictionary<T>) _current[typeof(T)];

            initializer.ID = _id;

            int count;
            typeSafeDictionary.GetValuesArray(out count)[typeSafeDictionary.FindElementIndex(_id.entityID)] = initializer;
        }

        readonly Dictionary<Type, ITypeSafeDictionary> _current;
        readonly EGID                                  _id;
    }
}