using Svelto.DataStructures.Experimental;

namespace Svelto.ECS.Internal
{
    partial class EntitiesDB
    {
        public void ExecuteOnEntity<T>(int id, ExclusiveGroup groupid, EntityAction<T> action) where T : IEntityStruct
        {
            ExecuteOnEntity(id, (int)groupid, action);
        }

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

        public void ExecuteOnEntity<T>(int id, int groupid, EntityAction<T> action) where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, groupid), action);
        }

        public void ExecuteOnEntity<T, W>(int id, int groupid, ref W value, EntityAction<T, W> action)
            where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, groupid), ref value, action);
        }

        public void ExecuteOnEntity<T, W>(int id, ExclusiveGroup groupid, ref W value, EntityAction<T, W> action) where T : IEntityStruct
        {
            ExecuteOnEntity(id, (int)groupid, ref value, action);
        }

        //----------------------------------------------------------------------------------------------------------

        public void ExecuteOnEntities<T>(int groupID, EntitiesAction<T> action) where T : IEntityStruct
        {
            int                   count;
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@groupID, out typeSafeDictionary) == false) return;

            var entities = typeSafeDictionary.GetValuesArray(out count);

            for (var i = 0; i < count; i++)
                action(ref entities[i], this, i);

            SafetyChecks(typeSafeDictionary, count);
        }

        public void ExecuteOnEntities<T>(ExclusiveGroup groupID, EntitiesAction<T> action) where T : IEntityStruct
        {
            ExecuteOnEntities((int)groupID, action);
        }

        public void ExecuteOnEntities<T, W>(int groupID, ref W value, EntitiesAction<T, W> action) where T : IEntityStruct
        {
            int                   count;
            TypeSafeDictionary<T> typeSafeDictionary;
            if (QueryEntitySafeDictionary(@groupID, out typeSafeDictionary) == false) return;

            var entities = typeSafeDictionary.GetValuesArray(out count);

            for (var i = 0; i < count; i++)
                action(ref entities[i], ref value, this, i);

            SafetyChecks(typeSafeDictionary, count);
        }

        public void ExecuteOnEntities<T, W>(ExclusiveGroup groupID, ref W value, EntitiesAction<T, W> action) where T : IEntityStruct
        {
            ExecuteOnEntities((int)groupID, ref value, action);
        }

        //-----------------------------------------------------------------------------------------------------------
        
        public void ExecuteOnAllEntities<T>(ExclusiveGroup[] damageableGroups, AllEntitiesAction<T> action) where T : IEntityStruct
        {
            var                                        type = typeof(T);
            FasterDictionary<int, ITypeSafeDictionary> dic;

            if (_groupedGroups.TryGetValue(type, out dic))
            {
                int count;
                var typeSafeDictionaries = dic.GetValuesArray(out count);

                for (int j = 0; j < count; j++)
                {
                    int innerCount;
                    var typeSafeDictionary = typeSafeDictionaries[j];
                    var casted             = typeSafeDictionary as TypeSafeDictionary<T>;

                    var entities = casted.GetValuesArray(out innerCount);

                    for (int i = 0; i < innerCount; i++)
                        action(ref entities[i], this);

                    SafetyChecks(casted, innerCount);
                }
            }
        }

        public void ExecuteOnAllEntities<T, W>(ref W value, AllEntitiesAction<T, W> action) where T : IEntityStruct
        {
            var                                        type = typeof(T);
            FasterDictionary<int, ITypeSafeDictionary> dic;

            if (_groupedGroups.TryGetValue(type, out dic))
            {
                int count;
                var typeSafeDictionaries = dic.GetValuesArray(out count);

                for (int j = 0; j < count; j++)
                {
                    int innerCount;
                    var typeSafeDictionary = typeSafeDictionaries[j];
                    var casted             = typeSafeDictionary as TypeSafeDictionary<T>;

                    var entities = casted.GetValuesArray(out innerCount);

                    for (int i = 0; i < innerCount; i++)
                        action(ref entities[i], ref value, this);

                    SafetyChecks(casted, innerCount);
                }
            }
        }

        public void ExecuteOnAllEntities<T>(ExclusiveGroup[] groups, EntitiesAction<T> action) where T : IEntityStruct
        {
            foreach (var group in groups)
            {
                ExecuteOnEntities(group, action);
            }
        }

        public void ExecuteOnAllEntities<T, W>(ExclusiveGroup[] groups, ref W value, EntitiesAction<T, W> action) where T : IEntityStruct
        {
            foreach (var group in groups)
            {
                ExecuteOnEntities(group, ref value, action);
            }
        }
    }
}