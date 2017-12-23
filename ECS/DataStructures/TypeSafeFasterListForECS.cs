using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeList: IEnumerable
    {
        void AddRange(ITypeSafeList entityViewListValue);

        ITypeSafeList Create();
        bool isQueryiableEntityView { get; }
        bool UnorderedRemove(int entityID);
        ITypeSafeDictionary CreateIndexedDictionary();
        IEntityView[] ToArrayFast(out int count);
    }

    class TypeSafeFasterListForECS<T>: FasterList<T> where T:IEntityView
    {
        protected TypeSafeFasterListForECS()
        {
            _mappedIndices = new Dictionary<int, int>();
        }
        
        public bool UnorderedRemove(int entityID)
        {
            var index = _mappedIndices[entityID];

            DesignByContract.Check.Assert(entityID == this[index].ID, "Something went wrong with the Svelto.ECS code, please contact the author");

            _mappedIndices.Remove(entityID);

            if (UnorderedRemoveAt(index))
                _mappedIndices[this[index].ID] = index;

            return this.Count > 0;
        }
        
        public void AddRange(ITypeSafeList entityViewListValue)
        {
            var index = this.Count;
            
            AddRange(entityViewListValue as FasterList<T>);
            
            for (int i = index; i < Count; ++i)
                _mappedIndices[this[i].ID] = i;
        }

        readonly Dictionary<int, int> _mappedIndices;
    }

    class TypeSafeFasterListForECSForStructs<T> : TypeSafeFasterListForECS<T>, ITypeSafeList where T:struct, IEntityStruct
    {
        public ITypeSafeList Create()
        {
            return new TypeSafeFasterListForECSForStructs<T>();
        }

        public bool isQueryiableEntityView
        {
            get { return false; }
        }

        public ITypeSafeDictionary CreateIndexedDictionary()
        {
            throw new Exception("Not Allowed");
        }

        public IEntityView[] ToArrayFast(out int count)
        {
            throw new Exception("Not Allowed");
        }
    }
    
    class TypeSafeFasterListForECSForClasses<T> : TypeSafeFasterListForECS<T>, ITypeSafeList where T:EntityView, new()
    {
        public ITypeSafeList Create()
        {
            return new TypeSafeFasterListForECSForClasses<T>();
        }

        public bool isQueryiableEntityView
        {
            get { return true; }
        }

        public ITypeSafeDictionary CreateIndexedDictionary()
        {
            return new TypeSafeDictionary<T>();
        }

        public IEntityView[] ToArrayFast(out int count)
        {
            count = this.Count;
            
            return this.ToArrayFast();
        }
    }
}
