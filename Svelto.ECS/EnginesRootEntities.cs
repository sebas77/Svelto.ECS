using System;
using System.Collections.Generic;
using DBC;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS
{
    public partial class EnginesRoot : IDisposable
    {
        public void Dispose()
        {
            foreach (var entity in _globalEntityViewsDB)
                if (entity.Value.isQueryiableEntityView)
                    foreach (var entityView in entity.Value)
                        RemoveEntityViewFromEngines(_entityViewEngines, entityView as EntityView, entity.Key);
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

        void BuildEntity<T>(int entityID, object[] implementors = null) where T : IEntityDescriptor, new()
        {
            BuildEntityInGroup<T>
                (entityID, ExclusiveGroups.StandardEntity, implementors);
        }

        void BuildEntity(int entityID, EntityDescriptorInfo entityDescriptor, object[] implementors)
        {
            BuildEntityInGroup
                (entityID, ExclusiveGroups.StandardEntity, entityDescriptor, implementors);
        }

        /// <summary>
        /// Build the entity using the entityID, inside the group with Id groupID, using the
        /// implementors (if necessary). The entityViews generated will be stored to be
        /// added later in the engines. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityID"></param>
        /// <param name="groupID"></param>
        /// <param name="implementors"></param>
        void BuildEntityInGroup<T>(int entityID, int groupID, object[] implementors = null)
            where T : IEntityDescriptor, new()
        {
            EntityFactory.BuildGroupedEntityViews(entityID, groupID,
                                                  _groupedEntityViewsToAdd.current,
                                                  EntityDescriptorTemplate<T>.Default,
                                                  _entityInfos,
                                                  implementors);
        }

        void BuildEntityInGroup(int entityID, int groupID, EntityDescriptorInfo entityDescriptor,
                                object[] implementors = null)
        {
            EntityFactory.BuildGroupedEntityViews(entityID, groupID,
                                                  _groupedEntityViewsToAdd.current,
                                                  entityDescriptor,
                                                  _entityInfos,
                                                   implementors);
        }

        ///--------------------------------------------

        /// <summary>
        /// This function is experimental and untested. I never used it in production
        /// it may not be necessary.
        /// TODO: understand if this method is useful in a performance critical
        /// scenario
        /// </summary>
        void Preallocate<T>(int groupID, int size) where T : IEntityDescriptor, new()
        {
            var entityViewsToBuild = EntityDescriptorTemplate<T>.Default.entityViewsToBuild;
            var count              = entityViewsToBuild.Length;

            for (var index = 0; index < count; index++)
            {
                var entityViewBuilder = entityViewsToBuild[index];
                var entityViewType    = entityViewBuilder.GetEntityViewType();

                //reserve space for the global pool
                ITypeSafeList dbList;
                if (_globalEntityViewsDB.TryGetValue(entityViewType, out dbList) == false)
                    _globalEntityViewsDB[entityViewType] = entityViewBuilder.Preallocate(ref dbList, size);
                else
                    dbList.AddCapacity(size);

                //reserve space for the single group
                Dictionary<Type, ITypeSafeList> @group;
                if (_groupEntityViewsDB.TryGetValue(groupID, out group) == false)
                    group = _groupEntityViewsDB[groupID] = new Dictionary<Type, ITypeSafeList>();
                
                if (group.TryGetValue(entityViewType, out dbList) == false)
                    group[entityViewType] = entityViewBuilder.Preallocate(ref dbList, size);
                else
                    dbList.AddCapacity(size);
                
                if (_groupedEntityViewsToAdd.current.TryGetValue(groupID, out group) == false)
                    group = _groupEntityViewsDB[groupID] = new Dictionary<Type, ITypeSafeList>();
                
                //reserve space to the temporary buffer
                if (group.TryGetValue(entityViewType, out dbList) == false)
                    group[entityViewType] = entityViewBuilder.Preallocate(ref dbList, size);
                else
                    dbList.AddCapacity(size);
            }
        }
        
        ///--------------------------------------------
        /// 
        void RemoveEntity(int entityID, int groupID)
        {
            RemoveEntity(new EGID(entityID, groupID));
        }

        void RemoveEntity(EGID entityGID)
        {
            var entityViewBuilders = _entityInfos[entityGID.GID];
            var entityViewBuildersCount = entityViewBuilders.Length;
            
            //for each entity view generated by the entity descriptor
            for (var i = 0; i < entityViewBuildersCount; i++)
            {
                var entityViewType = entityViewBuilders[i].GetEntityViewType();

                if (entityViewBuilders[i].isQueryiableEntityView)
                {
                    var group = _groupEntityViewsDB[entityGID.group];
                    InternalRemoveEntityViewFromDBDicAndEngines(entityViewType, entityGID);
                    RemoveEntityViewFromDB(@group, entityViewType, entityGID);
                }

                RemoveEntityViewFromDB(@_globalEntityViewsDB, entityViewType, entityGID);
            }

            _entityInfos.Remove(entityGID.GID);
        }

        static void RemoveEntityViewFromDB(Dictionary<Type, ITypeSafeList> @group, Type entityViewType, EGID id)
        {
            //remove it from entity views group DB
            var typeSafeList = @group[entityViewType];
            if (typeSafeList.MappedRemove(id) == false) //clean up
                @group.Remove(entityViewType);
        }

        void RemoveGroupAndEntitiesFromDB(int groupID)
        {
            foreach (var group in _groupEntityViewsDB[groupID])
            {
                {
                    var entityViewType = group.Key;

                    int count;
                    var entities = group.Value.ToArrayFast(out count);

                    for (var i = 0; i < count; i++)
                    {
                        var entityID = entities[i].ID;

                        RemoveEntityViewFromDB(@_globalEntityViewsDB, entityViewType, entityID);

                        if (group.Value.isQueryiableEntityView)
                            InternalRemoveEntityViewFromDBDicAndEngines(entityViewType, entityID);
                    }
                }
            }

            _groupEntityViewsDB.Remove(groupID);
        }

        void InternalRemoveEntityViewFromDBDicAndEngines(Type entityViewType, EGID id)
        {
            var typeSafeDictionary = _globalEntityViewsDBDic[entityViewType];
            {
                var entityView = typeSafeDictionary.GetIndexedEntityView(id);

                //the reason why this for exists is because in the past hierarchical entity views
                //where supported :(
                //Only EntityView can be removed from engines (won't work for IEntityStruct or IEntityView)
                for (var current = entityViewType; current != _entityViewType; current = current.BaseType)
                {
#if DEBUG && !PROFILER                    
                    if (current != entityViewType)
                        Utility.Console.LogWarning("Hierarchical Entity Views are design mistakes, ECS is not OOD!!");
#endif                        
                        
                    RemoveEntityViewFromEngines(_entityViewEngines, entityView, current);
                }
            }
            typeSafeDictionary.Remove(id);
        }
        
        static void RemoveEntityViewFromEngines(Dictionary<Type, FasterList<IHandleEntityViewEngine>> entityViewEngines,
                                                IEntityView                                           entityView,
                                                Type                                                  entityViewType)
        {
            FasterList<IHandleEntityViewEngine> enginesForEntityView;

            if (entityViewEngines.TryGetValue(entityViewType, out enginesForEntityView))
            {
                int count;
                var fastList = FasterList<IHandleEntityViewEngine>.NoVirt.ToArrayFast(enginesForEntityView, out count);

                for (var j = 0; j < count; j++)
                {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
                    EngineProfiler.MonitorRemoveDuration(fastList[j], entityView);
#else
                    fastList[j].Remove(entityView);
#endif
                }
            }
        }
        
        ///--------------------------------------------

        void SwapEntityGroup(int entityID, int fromGroupID, int toGroupID)
        {
            Check.Require(fromGroupID != toGroupID,
                          "can't move an entity to the same group where it already belongs to");

            var entityegid = new EGID(entityID, fromGroupID);
            var entityViewBuilders = _entityInfos[entityegid.GID];
            var entityViewBuildersCount = entityViewBuilders.Length;

            var groupedEntities = _groupEntityViewsDB[fromGroupID];

            Dictionary<Type, ITypeSafeList> groupedEntityViewsTyped;
            if (_groupEntityViewsDB.TryGetValue(toGroupID, out groupedEntityViewsTyped) == false)
            {
                groupedEntityViewsTyped = new Dictionary<Type, ITypeSafeList>();

                _groupEntityViewsDB.Add(toGroupID, groupedEntityViewsTyped);
            }

            for (var i = 0; i < entityViewBuildersCount; i++)
            {
                var entityViewBuilder = entityViewBuilders[i];
                var entityViewType    = entityViewBuilder.GetEntityViewType();

                var           fromSafeList = groupedEntities[entityViewType];
                ITypeSafeList toSafeList;

                if (groupedEntityViewsTyped.TryGetValue(entityViewType, out toSafeList) == false)
                    groupedEntityViewsTyped[entityViewType] = toSafeList = fromSafeList.Create();

                entityViewBuilder.MoveEntityView(entityegid, fromSafeList, toSafeList);
                fromSafeList.MappedRemove(entityegid);
            }

            _entityInfos.Remove(entityegid.GID);
            _entityInfos.Add(new EGID(entityID, toGroupID).GID, entityViewBuilders);
        }

        readonly EntityViewsDB _DB;
        
        //grouped set of entity views, this is the standard way to handle entity views
        readonly Dictionary<int, Dictionary<Type, ITypeSafeList>>         _groupEntityViewsDB;
        
        
        //TODO: Use faster dictionary and merge these two?
        
        //Global pool of entity views when engines want to manage entityViews regardless
        //the group
        readonly Dictionary<Type, ITypeSafeList>        _globalEntityViewsDB;
        //indexable entity views when the entity ID is known. Usually useful to handle
        //event based logic.
        readonly Dictionary<Type, ITypeSafeDictionary>  _globalEntityViewsDBDic;
        readonly Dictionary<long, IEntityViewBuilder[]> _entityInfos;
    }
}