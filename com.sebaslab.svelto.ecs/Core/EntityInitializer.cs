using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly ref struct EntityInitializer
    {
        public EntityInitializer(EGID id, FasterDictionary<RefWrapperType, ITypeSafeDictionary> group,
            in EntityReference reference)
        {
            _group = group;
            _ID = id;
            this.reference = reference;
        }

        public EGID EGID => _ID;
        public readonly EntityReference reference;

        public void Init<T>(T initializer) where T : struct, _IInternalEntityComponent
        {
            if (_group.TryGetValue(
                    new RefWrapperType(TypeCache<T>.type),
                    out var typeSafeDictionary) == false)
                return;

            var dictionary = (ITypeSafeDictionary<T>)typeSafeDictionary;
#if SLOW_SVELTO_SUBMISSION
            if (ComponentBuilder<T>.HAS_EGID)
                SetEGIDWithoutBoxing<T>.SetIDWithoutBoxing(ref initializer, _ID);
#endif

            if (dictionary.TryFindIndex(_ID.entityID, out var findElementIndex))
                dictionary.GetDirectValueByRef(findElementIndex) = initializer;
        }

        internal ref T GetOrAdd<T>() where T : unmanaged, IEntityComponent
        {
            ref var entityDictionary = ref _group.GetOrAdd(
                new RefWrapperType(ComponentBuilder<T>.ENTITY_COMPONENT_TYPE),
                () => new UnmanagedTypeSafeDictionary<T>(1));
            var dictionary = (ITypeSafeDictionary<T>)entityDictionary;

            return ref dictionary.GetOrAdd(_ID.entityID);
        }

        public ref T Get<T>() where T : struct, _IInternalEntityComponent
        {
            return ref (_group[new RefWrapperType(TypeCache<T>.type)] as ITypeSafeDictionary<T>)
               .GetValueByRef(_ID.entityID);
        }

        public bool Has<T>() where T : struct, _IInternalEntityComponent
        {
            if (_group.TryGetValue(
                    new RefWrapperType(TypeCache<T>.type),
                    out var typeSafeDictionary))
            {
                var dictionary = (ITypeSafeDictionary<T>)typeSafeDictionary;

                if (dictionary.ContainsKey(_ID.entityID))
                    return true;
            }

            return false;
        }

        readonly EGID _ID;
        readonly FasterDictionary<RefWrapperType, ITypeSafeDictionary> _group;
    }
}