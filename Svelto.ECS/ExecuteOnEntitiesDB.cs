namespace Svelto.ECS.Internal
{
    partial class EntitiesDB
    {
        public void ExecuteOnEntity<T>(int                                 id,
                                       ExclusiveGroup.ExclusiveGroupStruct groupid,
                                       EntityAction<T>                     action) where T : IEntityStruct
        {
            ExecuteOnEntity(id, (int)groupid, action);
        }

        public void ExecuteOnEntity<T, W>(EGID entityGID, ref W value, EntityAction<T, W> action) where T: IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (QueryEntitySafeDictionary(entityGID.groupID, out casted))
            {
                if (casted != null)
                    if (casted.ExecuteOnEntityView(entityGID.entityID, ref value, action))
                        return;
            }

            throw new EntityNotFoundException(entityGID.entityID, entityGID.groupID, typeof(T));
        }

        public void ExecuteOnEntity<T>(EGID entityGID, EntityAction<T> action) where T : IEntityStruct
        {
            TypeSafeDictionary<T> casted;
            if (QueryEntitySafeDictionary(entityGID.groupID, out casted))
            {
                if (casted != null)
                    if (casted.ExecuteOnEntityView(entityGID.entityID, action))
                        return;
            }

            throw new EntityNotFoundException(entityGID.entityID, entityGID.groupID, typeof(T));
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

        public void ExecuteOnEntity<T, W>(int                                 id,
                                          ExclusiveGroup.ExclusiveGroupStruct groupid,
                                          ref W                               value,
                                          EntityAction<T, W>                  action) where T : IEntityStruct
        {
            ExecuteOnEntity(id, (int)groupid, ref value, action);
        }

        //----------------------------------------------------------------------------------------------------------

        public void ExecuteOnEntities<T>(int groupID, EntitiesAction<T> action) where T : IEntityStruct
        {
            if (QueryEntitySafeDictionary(groupID, out TypeSafeDictionary<T> typeSafeDictionary) == false) return;

            var entities = typeSafeDictionary.GetValuesArray(out var count);

            for (var i = 0; i < count; i++)
                action(ref entities[i], new EntityActionData(this, i));

            SafetyChecks(typeSafeDictionary, count);
        }

        public void ExecuteOnEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId,
                                         EntitiesAction<T>                   action) where T : IEntityStruct
        {
            ExecuteOnEntities((int)groupStructId, action);
        }

        public void ExecuteOnEntities<T, W>(int groupID, ref W value, EntitiesAction<T, W> action) where T:IEntityStruct
        {
            if (QueryEntitySafeDictionary(groupID, out TypeSafeDictionary<T> typeSafeDictionary) == false) return;

            var entities = typeSafeDictionary.GetValuesArray(out var count);

            for (var i = 0; i < count; i++)
                action(ref entities[i], ref value, new EntityActionData(this, i));

            SafetyChecks(typeSafeDictionary, count);
        }

        public void ExecuteOnEntities<T, W>(ExclusiveGroup.ExclusiveGroupStruct groupStructId,
                                            ref W value, EntitiesAction<T, W> action) where T : IEntityStruct
        {
            ExecuteOnEntities((int)groupStructId, ref value, action);
        }

        //-----------------------------------------------------------------------------------------------------------
        
        public void ExecuteOnAllEntities<T>(AllEntitiesAction<T> action) where T : IEntityStruct
        {
            var type = typeof(T);

            if (_groupedGroups.TryGetValue(type, out var dic))
            {
                var typeSafeDictionaries = dic.GetValuesArray(out var count);

                for (int j = 0; j < count; j++)
                {
                    var typeSafeDictionary = typeSafeDictionaries[j];
                    var casted             = typeSafeDictionary as TypeSafeDictionary<T>;

                    var entities = casted.GetValuesArray(out var innerCount);

                    for (int i = 0; i < innerCount; i++)
                        action(ref entities[i], this);

                    SafetyChecks(casted, innerCount);
                }
            }
        }

        public void ExecuteOnAllEntities<T, W>(ref W value, AllEntitiesAction<T, W> action) where T : IEntityStruct
        {
            var type = typeof(T);

            if (_groupedGroups.TryGetValue(type, out var dic))
            {
                var typeSafeDictionaries = dic.GetValuesArray(out var count);

                for (int j = 0; j < count; j++)
                {
                    var typeSafeDictionary = typeSafeDictionaries[j];
                    var casted             = typeSafeDictionary as TypeSafeDictionary<T>;

                    var entities = casted.GetValuesArray(out var innerCount);

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

        public void ExecuteOnAllEntities<T, W>(ExclusiveGroup[]     groups,
                                               ref W                value,
                                               EntitiesAction<T, W> action) where T : IEntityStruct
        {
            foreach (var group in groups)
            {
                ExecuteOnEntities(group, ref value, action);
            }
        }
    }
}