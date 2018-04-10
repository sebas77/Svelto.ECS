using System;
using System.Collections;
using System.Collections.Generic;
using DBC;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeList : IEnumerable
    {
        bool isQueryiableEntityView { get; }
        void AddRange(ITypeSafeList entityViewListValue);

        ITypeSafeList       Create();
        bool                MappedRemove(EGID entityID);
        ITypeSafeDictionary CreateIndexedDictionary();
        IEntityView[]       ToArrayFast(out int count);
        void                AddCapacity(int capacity);
    }

    class TypeSafeFasterListForECS<T> : FasterList<T> where T : IEntityView
    {
        readonly Dictionary<long, int> _mappedIndices;

        protected TypeSafeFasterListForECS()
        {
            _mappedIndices = new Dictionary<long, int>();
        }

        protected TypeSafeFasterListForECS(int size) : base(size)
        {
            _mappedIndices = new Dictionary<long, int>();
        }

        public bool MappedRemove(EGID entityID)
        {
            var index = _mappedIndices[entityID.GID];

            Check.Assert(entityID.GID == this[index].ID.GID,
                         "Something went wrong with the Svelto.ECS code, please contact the author");

            _mappedIndices.Remove(entityID.GID);

            if (UnorderedRemoveAt(index))
            {
                _mappedIndices[this[index].ID.GID] = index;
            }

            return Count > 0;
        }

        public void AddRange(ITypeSafeList entityViewListValue)
        {
            var index = Count;

            base.AddRange(entityViewListValue as FasterList<T>);


            for (var i = index; i < Count; ++i)
            {
                try
                {
                    _mappedIndices.Add(this[i].ID.GID, i);
                }
                catch (Exception e)
                {
                    throw new TypeSafeFasterListForECSException(e);
                }
            }
        }

        public new void Add(T entityView)
        {
            var index = Count;

            base.Add(entityView);

            try
            {
                _mappedIndices.Add(entityView.ID.GID, index);
            }
            catch (Exception e)
            {
                throw new TypeSafeFasterListForECSException(e);
            }
        }

        public void AddCapacity(int capacity)
        {
            if (ToArrayFast().Length < Count + capacity)
                Resize(Count + capacity);
        }

        public int GetIndexFromID(EGID entityID)
        {
            return _mappedIndices[entityID.GID];
        }
    }

    class TypeSafeFasterListForECSForStructs<T> : TypeSafeFasterListForECS<T>, ITypeSafeList
        where T : struct, IEntityStruct
    {
        public TypeSafeFasterListForECSForStructs(int size) : base(size)
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
            throw new NotSupportedException();
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
            return new TypeSafeDictionaryForClass<T>();
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