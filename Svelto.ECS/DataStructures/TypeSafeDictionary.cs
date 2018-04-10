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
        bool        Remove(EGID entityId);
        IEntityView GetIndexedEntityView(EGID entityID);
    }

    class TypeSafeDictionaryForClass<TValue> : Dictionary<long, TValue>, ITypeSafeDictionary where TValue : EntityView
    {
        internal static readonly ReadOnlyDictionary<long, TValue> Default =
            new ReadOnlyDictionary<long, TValue>(new Dictionary<long, TValue>());

        public void FillWithIndexedEntityViews(ITypeSafeList entityViews)
        {
            int count;
            var buffer = FasterList<TValue>.NoVirt.ToArrayFast((FasterList<TValue>) entityViews, out count);

            try
            {
                for (var i = 0; i < count; i++)
                {
                    var entityView = buffer[i];

                    Add(entityView._ID.GID, entityView);
                }
            }
            catch (Exception e)
            {
                throw new TypeSafeDictionaryException(e);
            }
        }

        public bool Remove(EGID entityId)
        {
            base.Remove(entityId.GID);

            return Count > 0;
        }

        public IEntityView GetIndexedEntityView(EGID entityID)
        {
            return this[entityID.GID];
        }
    }
}