using Svelto.DataStructures;
using System.Collections.Generic;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
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
        void FillWithIndexedNodes(ITypeSafeList nodes);
        void Remove(int entityId);
        NodeWithID GetIndexedNode(int entityID);
    }

    class TypeSafeDictionary<TValue> : Dictionary<int, TValue>, ITypeSafeDictionary where TValue:NodeWithID
    {
        internal static readonly ReadOnlyDictionary<int, TValue> Default = 
            new ReadOnlyDictionary<int, TValue>(new Dictionary<int, TValue>());
        
        public void FillWithIndexedNodes(ITypeSafeList nodes)
        {
            int count;
            var buffer = FasterList<TValue>.NoVirt.ToArrayFast((FasterList<TValue>) nodes, out count);

            for (int i = 0; i < count; i++)
            {
                var node = buffer[i];

                Add(node.ID, node);
            }
        }

        public void Remove(int entityId)
        {
            throw new System.NotImplementedException();
        }

        public NodeWithID GetIndexedNode(int entityID)
        {
            return this[entityID];
        }
    }
}
