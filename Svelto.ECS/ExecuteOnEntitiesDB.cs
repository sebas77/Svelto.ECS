using Svelto.DataStructures.Experimental;
using Svelto.Utilities;

namespace Svelto.ECS.Internal
{
    partial class entitiesDB
    {
        public void ExecuteOnEntity<T, W>(EGID entityGID, ref W value, EntityAction<T, W> action) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (QueryEntitySafeDictionary(entityGID.groupID, out casted))
            {
                if (casted != null)
                    if (casted.ExecuteOnEntityView(entityGID.entityID, ref value, action) == true)
                        return;
            }

            throw new EntitiesDBException("Entity not found id: "
                                         .FastConcat(entityGID.entityID).FastConcat(" groupID: ")
                                         .FastConcat(entityGID.groupID));
        }

        public void ExecuteOnEntity<T>(EGID entityGID, EntityAction<T> action) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (QueryEntitySafeDictionary(entityGID.groupID, out casted))
            {
                if (casted != null)
                    if (casted.ExecuteOnEntityView(entityGID.entityID, action) == true)
                        return;
            }

            throw new EntitiesDBException("Entity not found id: "
                                         .FastConcat(entityGID.entityID).FastConcat(" groupID: ")
                                         .FastConcat(entityGID.groupID));
        }

        public void ExecuteOnEntity<T>(int id, EntityAction<T> action) where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, ExclusiveGroup.StandardEntitiesGroup), action);
        }

        public void ExecuteOnEntity<T>(int id, int groupid, EntityAction<T> action) where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, groupid), action);
        }

        public void ExecuteOnEntity<T, W>(int id, ref W value, EntityAction<T, W> action) where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, ExclusiveGroup.StandardEntitiesGroup), ref value, action);
        }

        public void ExecuteOnEntity<T, W>(int id, int groupid, ref W value, EntityAction<T, W> action)
            where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, groupid), ref value, action);
        }
        
        //----------------------------------------------------------------------------------------------------------

        public void ExecuteOnEntities<T>(int groupID, EntitiesAction<T> action) where T : IEntityStruct
        {
            int                   count;
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@groupID, out typeSafeDictionary) == false) return;

            var entities = typeSafeDictionary.GetFasterValuesBuffer(out count);

            for (var i = 0; i < count; i++)
                action(ref entities[i], i);

            SafetyChecks(typeSafeDictionary, count);
        }

        public void ExecuteOnEntities<T>(EntitiesAction<T> action) where T : IEntityStruct
        {
            ExecuteOnEntities(ExclusiveGroup.StandardEntitiesGroup, action);
        }

        public void ExecuteOnEntities<T, W>(int groupID, ref W value, EntitiesAction<T, W> action) where T : IEntityStruct
        {
            int                   count;
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@groupID, out typeSafeDictionary) == false) return;

            var entities = typeSafeDictionary.GetFasterValuesBuffer(out count);

            for (var i = 0; i < count; i++)
                action(ref entities[i], ref value, i);

            SafetyChecks(typeSafeDictionary, count);
        }

        public void ExecuteOnEntities<T, W>(ref W value, EntitiesAction<T, W> action) where T : IEntityStruct
        {
            ExecuteOnEntities(ExclusiveGroup.StandardEntitiesGroup, ref value, action);
        }

        public void ExecuteOnEntities<T, T1, W>(W value, EntitiesAction<T, T1, W> action)
            where T : IEntityStruct where T1 : IEntityStruct
        {
            ExecuteOnEntities(ExclusiveGroup.StandardEntitiesGroup, ref value, action);
        }

        public void ExecuteOnEntities<T, T1>(int group, EntitiesAction<T, T1> action)
            where T : IEntityStruct where T1 : IEntityStruct
        {
            int                   count;
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@group, out typeSafeDictionary) == false) return;

            var entities = typeSafeDictionary.GetFasterValuesBuffer(out count);

            EGIDMapper<T1> map = QueryMappedEntities<T1>(@group);

            for (var i = 0; i < count; i++)
            {
                uint index;
                action(ref entities[i], ref map.entities(entities[i].ID, out index)[index], i);
            }

            SafetyChecks(typeSafeDictionary, count);
        }

        public void ExecuteOnEntities<T, T1, W>(int group, ref W value, EntitiesAction<T, T1, W> action)
            where T : IEntityStruct where T1 : IEntityStruct
        {
            int                   count;
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@group, out typeSafeDictionary) == false) return;

            var entities = typeSafeDictionary.GetFasterValuesBuffer(out count);

            EGIDMapper<T1> map = QueryMappedEntities<T1>(@group);

            for (var i = 0; i < count; i++)
            {
                uint index;
                action(ref entities[i], ref map.entities(entities[i].ID, out index)[index], ref value, i);
            }

            SafetyChecks(typeSafeDictionary, count);
        }

        public void ExecuteOnEntities<T, T1>(EntitiesAction<T, T1> action) where T : IEntityStruct where T1 : IEntityStruct
        {
            ExecuteOnEntities(ExclusiveGroup.StandardEntitiesGroup, action);
        }

        public void ExecuteOnEntities<T, T1, W>(ref W value, EntitiesAction<T, T1, W> action)
            where T : IEntityStruct where T1 : IEntityStruct
        {
            ExecuteOnEntities(ExclusiveGroup.StandardEntitiesGroup, ref value, action);
        }
        
        //-----------------------------------------------------------------------------------------------------------
        
        public void ExecuteOnAllEntities<T>(EntityAction<T> action) where T : IEntityStruct
        {
            var                                        type = typeof(T);
            FasterDictionary<int, ITypeSafeDictionary> dic;

            if (_groupedGroups.TryGetValue(type, out dic))
            {
                int count;
                var typeSafeDictionaries = dic.GetFasterValuesBuffer(out count);

                for (int j = 0; j < count; j++)
                {
                    int innerCount;
                    var typeSafeDictionary = typeSafeDictionaries[j];
                    var casted             = typeSafeDictionary as TypeSafeDictionary<T>;

                    var entities = casted.GetFasterValuesBuffer(out innerCount);

                    for (int i = 0; i < innerCount; i++)
                        action(ref entities[i]);

                    SafetyChecks(casted, count);
                }
            }
        }

        public void ExecuteOnAllEntities<T, W>(ref W value, EntityAction<T, W> action) where T : IEntityStruct
        {
            var                                        type = typeof(T);
            FasterDictionary<int, ITypeSafeDictionary> dic;

            if (_groupedGroups.TryGetValue(type, out dic))
            {
                int count;
                var typeSafeDictionaries = dic.GetFasterValuesBuffer(out count);

                for (int j = 0; j < count; j++)
                {
                    int innerCount;
                    var typeSafeDictionary = typeSafeDictionaries[j];
                    var casted             = typeSafeDictionary as TypeSafeDictionary<T>;

                    var entities = casted.GetFasterValuesBuffer(out innerCount);

                    for (int i = 0; i < innerCount; i++)
                        action(ref entities[i], ref value);

                    SafetyChecks(casted, count);
                }
            }
        }


    }
}