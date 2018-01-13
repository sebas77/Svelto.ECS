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
        void ReserveCapacity(int capacity);
    }

    class TypeSafeFasterListForECS<T>: FasterList<T> where T:IEntityView
    {
        protected TypeSafeFasterListForECS()
        {
            _mappedIndices = new Dictionary<int, int>();
        }

        protected TypeSafeFasterListForECS(int size):base(size)
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
            
            base.AddRange(entityViewListValue as FasterList<T>);
            
            for (int i = index; i < Count; ++i)
                _mappedIndices[this[i].ID] = i;
        }

        new public void Add(T entityView)
        {
            var index = this.Count;

            base.Add(entityView);

            _mappedIndices[entityView.ID] = index;
        }

        public void ReserveCapacity(int capacity)
        {
            if (this.ToArrayFast().Length < capacity)
                Resize(capacity);
        }

        public int GetIndexFromID(int entityID)
        {
            return _mappedIndices[entityID];
        }

        readonly Dictionary<int, int> _mappedIndices;
    }

    class TypeSafeFasterListForECSForStructs<T> : TypeSafeFasterListForECS<T>, ITypeSafeList where T:struct, IEntityStruct
    {
        public TypeSafeFasterListForECSForStructs(int size):base(size)
        {}

        public TypeSafeFasterListForECSForStructs()
        {}

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

        public ITypeSafeList Create(int size)
        {
            return new TypeSafeFasterListForECSForStructs<T>(size);
        }
    }
    
    class TypeSafeFasterListForECSForClasses<T> : TypeSafeFasterListForECS<T>, ITypeSafeList where T:EntityView, new()
    {
        public TypeSafeFasterListForECSForClasses(int size):base(size)
        {}

        public TypeSafeFasterListForECSForClasses()
        {}

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

        public ITypeSafeList Create(int size)
        {
            return new TypeSafeFasterListForECSForClasses<T>(size);
        }
    }
}
