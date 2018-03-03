using System;
using System.Collections;
using System.Collections.Generic;
using DesignByContract;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeList : IEnumerable
    {
        bool isQueryiableEntityView { get; }
        void AddRange(ITypeSafeList entityViewListValue);

        ITypeSafeList       Create();
        bool                MappedRemove(int entityID);
        ITypeSafeDictionary CreateIndexedDictionary();
        IEntityView[]       ToArrayFast(out int count);
        void                ReserveCapacity(int capacity);
    }

    class TypeSafeFasterListForECS<T> : FasterList<T> where T : IEntityView
    {
        readonly Dictionary<int, int> _mappedIndices;

        protected TypeSafeFasterListForECS()
        {
            _mappedIndices = new Dictionary<int, int>();
        }

        protected TypeSafeFasterListForECS(int size) : base(size)
        {
            _mappedIndices = new Dictionary<int, int>();
        }

        public bool MappedRemove(int entityID)
        {
            var index = _mappedIndices[entityID];

            Check.Assert(entityID == this[index].ID,
                         "Something went wrong with the Svelto.ECS code, please contact the author");

            _mappedIndices.Remove(entityID);

            if (UnorderedRemoveAt(index))
                _mappedIndices[this[index].ID] = index;

            return Count > 0;
        }

        public void AddRange(ITypeSafeList entityViewListValue)
        {
            var index = Count;

            base.AddRange(entityViewListValue as FasterList<T>);

            for (var i = index; i < Count; ++i)
                _mappedIndices[this[i].ID] = i;
        }

        public new void Add(T entityView)
        {
            var index = Count;

            base.Add(entityView);

            _mappedIndices[entityView.ID] = index;
        }

        public void ReserveCapacity(int capacity)
        {
            if (ToArrayFast().Length < capacity)
                Resize(capacity);
        }

        public int GetIndexFromID(int entityID)
        {
            return _mappedIndices[entityID];
        }
    }

    class TypeSafeFasterListForECSForStructs<T> : TypeSafeFasterListForECS<T>, ITypeSafeList
        where T : struct, IEntityStruct
    {
        public TypeSafeFasterListForECSForStructs(int size) : base(size)
        {
        }

        public TypeSafeFasterListForECSForStructs()
        {
        }

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

    class TypeSafeFasterListForECSForClasses<T> : TypeSafeFasterListForECS<T>, ITypeSafeList where T : EntityView, new()
    {
        public TypeSafeFasterListForECSForClasses(int size) : base(size)
        {
        }

        public TypeSafeFasterListForECSForClasses()
        {
        }

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
            count = Count;

            return ToArrayFast();
        }

        public ITypeSafeList Create(int size)
        {
            return new TypeSafeFasterListForECSForClasses<T>(size);
        }
    }
}