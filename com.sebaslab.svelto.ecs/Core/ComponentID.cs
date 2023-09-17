using System;
using System.Diagnostics;

namespace Svelto.ECS
{
    sealed class ComponentIDDebugProxy
    {
        public ComponentIDDebugProxy(ComponentID id)
        {
            this._id = id;
        }

        public Type type => ComponentTypeMap.FetchType(_id);

        readonly ComponentID _id;
    }


    [DebuggerTypeProxy(typeof(ComponentIDDebugProxy))]
    public struct ComponentID: IEquatable<ComponentID>, IComparable<ComponentID>
    {
        public static implicit operator int(ComponentID id)
        {
            return id._id;
        }

        public static implicit operator uint(ComponentID id)
        {
            return (uint)id._id;
        }

        public static implicit operator ComponentID(int id)
        {
            return new ComponentID()
            {
                _id = id
            };
        }

        public bool Equals(ComponentID other)
        {
            return _id == other._id;
        }

        public override int GetHashCode()
        {
            return _id;
        }
        
        public int CompareTo(ComponentID other)
        {
            return _id.CompareTo(other._id);
        }

        int _id;
    }
}