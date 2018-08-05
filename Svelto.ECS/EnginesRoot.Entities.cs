using System;
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

        EntityStructInitializer BuildEntity(EGID entityID, 
                                IEntityDescriptor entityDescriptor,
                                object[] implementors)
        {
            var descriptorEntitiesToBuild = entityDescriptor.entitiesToBuild;
            
#if DEBUG && !PROFILER            
            CheckEntityID(entityID, entityDescriptor);
#endif            
            var dic = EntityFactory.BuildGroupedEntityViews(entityID,
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
        void MoveEntity<T>(EGID entityGID, int toGroupID = -1, Dictionary<Type, ITypeSafeDictionary> toGroup = null) where T:IEntityDescriptor, new ()
        {
            var entityBuilders = EntityDescriptorTemplate<T>.descriptor.entitiesToBuild;
            var entityBuildersCount = entityBuilders.Length;
           
            //for each entity view generated by the entity descriptor
            for (var i = 0; i < entityBuildersCount; i++)
            {
                var entityType = entityBuilders[i].GetEntityType();
                
                MoveEntity(entityGID, toGroupID, toGroup, entityType);
            }
        }

        void MoveEntity(EGID fromEntityGID, int toGroupID, Dictionary<Type, ITypeSafeDictionary> toGroup, Type entityType)
        {
            var fromGroup = _groupEntityDB[fromEntityGID.groupID];

            var fromTypeSafeDictionary = fromGroup[entityType];
            ITypeSafeDictionary safeDictionary = null;

            if (toGroup != null)
            {
                if (toGroup.TryGetValue(entityType, out safeDictionary) == false)
                {
                    safeDictionary = fromTypeSafeDictionary.Create();
                    toGroup.Add(entityType, safeDictionary);
                    _groupedGroups[entityType] = new FasterDictionary<int, ITypeSafeDictionary>();
                }

                _groupedGroups[entityType][toGroupID] = safeDictionary;
            }

            fromTypeSafeDictionary.MoveEntityFromDictionaryAndEngines(fromEntityGID, toGroupID, safeDictionary, _entityEngines);

            if (fromTypeSafeDictionary.Count == 0) //clean up
            {
                _groupedGroups[entityType].Remove(fromEntityGID.groupID);

                //it's probably better to not remove this, but the dictionary should be trimmed?
                //fromGroup.Remove(entityType);
                fromTypeSafeDictionary.Trim();
            }
            
            //it doesn't eliminate the fromGroup itself on purpose
        }

        void RemoveGroupAndEntitiesFromDB(int groupID)
        {
            foreach (var entiTypeSafeList in _groupEntityDB[groupID])
                entiTypeSafeList.Value.RemoveEntitiesFromEngines(_entityEngines);

            _groupEntityDB.Remove(groupID);
        }

        ///--------------------------------------------

        EGID SwapEntityGroup<T>(int entityID, int fromGroupID, int toGroupID) where T:IEntityDescriptor, new ()
        {
            DBC.ECS.Check.Require(fromGroupID != toGroupID,
                          "the entity is already in this group");

            Dictionary<Type, ITypeSafeDictionary> toGroup;

            if (_groupEntityDB.TryGetValue(toGroupID, out toGroup) == false)
                toGroup = _groupEntityDB[toGroupID] = new Dictionary<Type, ITypeSafeDictionary>();

            MoveEntity<T>(new EGID(entityID, fromGroupID), toGroupID, toGroup);
            
            return new EGID(entityID, toGroupID);
        }
        
        EGID SwapFirstEntityInGroup<T>(int fromGroupID, int toGroupId) where T:IEntityDescriptor, new()
        {
            var firstID = _groupEntityDB[fromGroupID][EntityDescriptorTemplate<T>.descriptor.entitiesToBuild[0]
                                                                                 .GetEntityType()].GetFirstID();
            
            SwapEntityGroup<T>(firstID, fromGroupID, toGroupId);
            
            return new EGID(firstID, toGroupId);
        }

        readonly entitiesDB         _DB;
        
        int                         _newEntitiesBuiltToProcess;
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
            typeSafeDictionary.GetFasterValuesBuffer(out count)[typeSafeDictionary.FindElementIndex(_id.entityID)] = initializer;
        }

        readonly Dictionary<Type, ITypeSafeDictionary> _current;
        readonly EGID                                  _id;
    }
}