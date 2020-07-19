using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly ref struct EntityComponentInitializer
    {
        public EntityComponentInitializer(EGID id, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> group)
        {
            _group = group;
            _ID = id;
        }

        public EGID EGID => _ID;

        public void Init<T>(T initializer) where T : struct, IEntityComponent
        {
            if (_group.TryGetValue(new RefWrapper<Type>(ComponentBuilder<T>.ENTITY_COMPONENT_TYPE),
                    out var typeSafeDictionary) == false) return;

            var dictionary = (ITypeSafeDictionary<T>) typeSafeDictionary;

            if (ComponentBuilder<T>.HAS_EGID)
                SetEGIDWithoutBoxing<T>.SetIDWithoutBoxing(ref initializer, _ID);

            if (dictionary.TryFindIndex(_ID.entityID, out var findElementIndex))
                dictionary.GetDirectValueByRef(findElementIndex) = initializer;
        }

        public ref T GetOrCreate<T>() where T : struct, IEntityComponent
        {
            ref var entityDictionary = ref _group.GetOrCreate(new RefWrapper<Type>(ComponentBuilder<T>.ENTITY_COMPONENT_TYPE)
            , TypeSafeDictionaryFactory<T>.Create);
            var dictionary = (ITypeSafeDictionary<T>) entityDictionary;

            return ref dictionary.GetOrCreate(_ID.entityID);
        }
        
        public ref T Get<T>() where T : struct, IEntityComponent
        {
            return ref (_group[new RefWrapper<Type>(ComponentBuilder<T>.ENTITY_COMPONENT_TYPE)] as ITypeSafeDictionary<T>)[
                _ID.entityID];
        }
        
        public bool Has<T>() where T : struct, IEntityComponent
        {
            if (_group.TryGetValue(new RefWrapper<Type>(ComponentBuilder<T>.ENTITY_COMPONENT_TYPE),
                out var typeSafeDictionary))
            {
                var dictionary = (ITypeSafeDictionary<T>) typeSafeDictionary;

                if (dictionary.ContainsKey(_ID.entityID))
                    return true;
            }

            return false;
        }
        
        readonly EGID                                                    _ID;
        readonly FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> _group;
    }
}