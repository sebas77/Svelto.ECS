using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EntityGroup
    {
        internal EntityGroup(FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> entitiesInGroupPerType, uint _groupID)
        {
            _group = entitiesInGroupPerType;
            groupID = _groupID;
        }

        public ref T QueryEntity<T>(uint entityGidEntityId) where T : struct, IEntityStruct
        {
            return ref (_group[new RefWrapper<Type>(typeof(T))] as TypeSafeDictionary<T>).GetValueByRef(
                entityGidEntityId);
        }

        public bool Exists<T>(uint entityGidEntityId) where T : struct, IEntityStruct
        {
            return (_group[new RefWrapper<Type>(typeof(T))] as TypeSafeDictionary<T>).ContainsKey(entityGidEntityId);
        }

        readonly FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> _group;
        public uint groupID;
    }
}