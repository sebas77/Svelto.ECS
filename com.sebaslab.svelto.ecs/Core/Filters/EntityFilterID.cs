using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    // This is just an opaque struct to identify filter collections.
    public struct EntityFilterID : IEquatable<EntityFilterID>
    {
        internal EntityFilterID(uint filterID, RefWrapperType componentType)
        {
            _filterID = filterID;
            _componentType = componentType;
            _hashCode = (int)filterID + (int)filterID ^ componentType.GetHashCode();
        }

        public bool Equals(EntityFilterID other)
        {
            return _filterID == other._filterID && _componentType.Equals(other._componentType);
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode() => _hashCode;

        readonly uint           _filterID;
        readonly RefWrapperType _componentType;
        readonly int            _hashCode;
    }
}