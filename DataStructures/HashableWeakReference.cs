using System;

namespace Svelto.DataStructures
{
    class HashableWeakRef<T> : IEquatable<HashableWeakRef<T>> where T : class
    {
        public bool isAlive { get { return _weakRef.IsAlive; } }
        public T Target { get { return (T)_weakRef.Target; } }

        public HashableWeakRef(T target)
        {
            _weakRef = new WeakReference(target);
            _hash = target.GetHashCode();
        }

        public static bool operator !=(HashableWeakRef<T> a, HashableWeakRef<T> b)
        {
            return !(a == b);
        }

        public static bool operator ==(HashableWeakRef<T> a, HashableWeakRef<T> b)
        {
            if (a._hash != b._hash)
                return false;

            var tmpTargetA = (T) a._weakRef.Target;
            var tmpTargetB = (T) b._weakRef.Target;

            if (tmpTargetA == null || tmpTargetB == null)
                return false;

            return tmpTargetA == tmpTargetB;
        }

        public override bool Equals(object other)
        {
            if (other is HashableWeakRef<T>)
                return this.Equals((HashableWeakRef<T>)other);

            return false;
        }

        public bool Equals(HashableWeakRef<T> other)
        {
            return (this == other);
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        int             _hash;
        WeakReference   _weakRef;
    }
}
