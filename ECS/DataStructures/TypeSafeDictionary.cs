using Svelto.DataStructures;
using System.Collections.Generic;

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
        void FillWithIndexedEntityViews(ITypeSafeList entityViews);
        bool Remove(int entityId);
        IEntityView GetIndexedEntityView(int entityID);
    }

    class TypeSafeDictionary<TValue> : Dictionary<int, TValue>, ITypeSafeDictionary where TValue:IEntityView
    {
        internal static readonly ReadOnlyDictionary<int, TValue> Default = 
            new ReadOnlyDictionary<int, TValue>(new Dictionary<int, TValue>());
        
        public void FillWithIndexedEntityViews(ITypeSafeList entityViews)
        {
            int count;
            var buffer = FasterList<TValue>.NoVirt.ToArrayFast((FasterList<TValue>) entityViews, out count);

            for (int i = 0; i < count; i++)
            {
                var entityView = buffer[i];

                Add(entityView.ID, entityView);
            }
        }

        new public bool Remove(int entityId)
        {
            base.Remove(entityId);

            return this.Count > 0;
        }

        public IEntityView GetIndexedEntityView(int entityID)
        {
            return this[entityID];
        }
    }
}
