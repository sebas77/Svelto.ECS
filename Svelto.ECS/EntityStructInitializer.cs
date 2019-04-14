using System;
using System.Collections.Generic;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EntityStructInitializer
    {
        public EntityStructInitializer(EGID id, Dictionary<Type, ITypeSafeDictionary> @group)
        {
            _group = @group;
            ID      = id;
        }

        public void Init<T>(T initializer) where T: struct, IEntityStruct
        {
            if (_group.TryGetValue(EntityBuilder<T>.ENTITY_VIEW_TYPE, out var typeSafeDictionary) == true)
            {
                var dictionary = typeSafeDictionary as TypeSafeDictionary<T>;

                if (EntityBuilder<T>.HAS_EGID)
                {
                    var needEgid = ((INeedEGID) initializer);
                    needEgid.ID = ID;
                    initializer = (T) needEgid;
                }

                if (dictionary.TryFindElementIndex(ID.entityID, out var findElementIndex))
                    dictionary.GetValuesArray(out _)[findElementIndex] = initializer;
            }
        }
        
        readonly EGID ID;
        readonly Dictionary<Type, ITypeSafeDictionary> _group;
    }
}