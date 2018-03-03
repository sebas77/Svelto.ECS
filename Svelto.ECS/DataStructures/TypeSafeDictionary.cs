using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    /// <summary>
    ///     This is just a place holder at the moment
    ///     I always wanted to create my own Dictionary
    ///     data structure as excercise, but never had the
    ///     time to. At the moment I need the custom interface
    ///     wrapped though.
    /// </summary>
    public interface ITypeSafeDictionary
    {
        void        FillWithIndexedEntityViews(ITypeSafeList entityViews);
        bool        Remove(int                               entityId);
        IEntityView GetIndexedEntityView(int                 entityID);
    }

    class TypeSafeDictionary<TValue> : Dictionary<int, TValue>, ITypeSafeDictionary where TValue : IEntityView
    {
        internal static readonly ReadOnlyDictionary<int, TValue> Default =
            new ReadOnlyDictionary<int, TValue>(new Dictionary<int, TValue>());

        public void FillWithIndexedEntityViews(ITypeSafeList entityViews)
        {
            int count;
            var buffer = FasterList<TValue>.NoVirt.ToArrayFast((FasterList<TValue>) entityViews, out count);

            try
            {
                for (var i = 0; i < count; i++)
                {
                    var entityView = buffer[i];

                    Add(entityView.ID, entityView);
                }
            }
            catch (Exception e)
            {
                throw new TypeSafeDictionaryException(e);
            }
        }

        public new bool Remove(int entityId)
        {
            base.Remove(entityId);

            return Count > 0;
        }

        public IEntityView GetIndexedEntityView(int entityID)
        {
            return this[entityID];
        }
    }
}