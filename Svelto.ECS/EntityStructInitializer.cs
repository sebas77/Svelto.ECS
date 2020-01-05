using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public ref struct EntityStructInitializer
    {
        public EntityStructInitializer(EGID id, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> group)
        {
            _group = group;
            _ID = id;
        }

        public void Init<T>(T initializer) where T : struct, IEntityStruct
        {
            if (_group.TryGetValue(new RefWrapper<Type>(EntityBuilder<T>.ENTITY_VIEW_TYPE),
                    out var typeSafeDictionary) == false) return;

            var dictionary = (TypeSafeDictionary<T>) typeSafeDictionary;

            if (EntityBuilder<T>.HAS_EGID)
                SetEGIDWithoutBoxing<T>.SetIDWithoutBoxing(ref initializer, _ID);

            if (dictionary.TryFindIndex(_ID.entityID, out var findElementIndex))
                dictionary.GetDirectValue(findElementIndex) = initializer;
        }
        
        public void CopyFrom<T>(T initializer) where T : struct, IEntityStruct
        {
            var dictionary = (TypeSafeDictionary<T>) _group[new RefWrapper<Type>(EntityBuilder<T>.ENTITY_VIEW_TYPE)];

            if (EntityBuilder<T>.HAS_EGID)
                SetEGIDWithoutBoxing<T>.SetIDWithoutBoxing(ref initializer, _ID);

           dictionary[_ID.entityID] = initializer;
        }

        public ref T GetOrCreate<T>() where T : struct, IEntityStruct
        {
            ref var entityDictionary = ref _group.GetOrCreate(new RefWrapper<Type>(EntityBuilder<T>.ENTITY_VIEW_TYPE)
            , () => new TypeSafeDictionary<T>());
            var dictionary = (TypeSafeDictionary<T>) entityDictionary;

            return ref dictionary.GetOrCreate(_ID.entityID);
        }
        
        public T Get<T>() where T : struct, IEntityStruct
        {
            return (_group[new RefWrapper<Type>(EntityBuilder<T>.ENTITY_VIEW_TYPE)] as TypeSafeDictionary<T>)[_ID.entityID];
        }

        public bool Has<T>() where T : struct, IEntityStruct
        {
            if (_group.TryGetValue(new RefWrapper<Type>(EntityBuilder<T>.ENTITY_VIEW_TYPE),
                out var typeSafeDictionary))
            {
                var dictionary = (TypeSafeDictionary<T>) typeSafeDictionary;

                if (dictionary.ContainsKey(_ID.entityID))
                    return true;
            }

            return false;
        }

        public static EntityStructInitializer CreateEmptyInitializer()
        {
            return new EntityStructInitializer(new EGID(), new FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>());
        }

        readonly EGID                                                    _ID;
        readonly FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> _group;
    }
}