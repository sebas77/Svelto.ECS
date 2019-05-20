using System;

namespace Svelto.ECS.Internal
{
    partial class EntitiesDB
    {
        public void ExecuteOnAllEntities<T>(Action<T[], ExclusiveGroup.ExclusiveGroupStruct, uint, IEntitiesDB> action)
            where T : struct, IEntityStruct
        {
            var type = typeof(T);

            if (_groupsPerEntity.TryGetValue(type, out var dic))
            {
                foreach (var pair in dic)
                {
                    var entities = (pair.Value as TypeSafeDictionary<T>).GetValuesArray(out var innerCount);

                    if (innerCount > 0)
                        action(entities, new ExclusiveGroup.ExclusiveGroupStruct(pair.Key), innerCount, this);
                }
            }
        }

        public void ExecuteOnAllEntities
            <T, W>(W value, Action<T[], ExclusiveGroup.ExclusiveGroupStruct, uint, IEntitiesDB, W> action)
            where T : struct, IEntityStruct
        {
            var type = typeof(T);

            if (_groupsPerEntity.TryGetValue(type, out var dic))
            {
                foreach (var pair in dic)
                {
                    var entities = (pair.Value as TypeSafeDictionary<T>).GetValuesArray(out var innerCount);

                    if (innerCount > 0)
                        action(entities, new ExclusiveGroup.ExclusiveGroupStruct(pair.Key), innerCount, this, value);
                }
            }
        }
    }
}