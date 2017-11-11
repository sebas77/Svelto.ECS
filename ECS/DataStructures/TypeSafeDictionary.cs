using Svelto.DataStructures;
using System.Collections.Generic;

namespace Svelto.ECS
{
    /// <summary>
    /// This is just a place holder at the moment
    /// I always wanted to create my own Dictionary
    /// data structure as excercise, but never had the
    /// time to. At the moment I need the custom interface
    /// wrapped though.
    /// </summary>

    public interface ITypeSafeDictionary
    {

    }

    public class TypeSafeDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ITypeSafeDictionary
    {
        internal static readonly ReadOnlyDictionary<TKey, TValue> Default = new ReadOnlyDictionary<TKey, TValue>(new TypeSafeDictionary<TKey, TValue>());
    }
}
