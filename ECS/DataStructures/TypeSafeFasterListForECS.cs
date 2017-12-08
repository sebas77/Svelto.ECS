using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeList: IEnumerable
    {
        void AddRange(ITypeSafeList nodeListValue);

        ITypeSafeList Create();
        bool isQueryiableNode { get; }
        void UnorderedRemove(int index);
        ITypeSafeDictionary CreateIndexedDictionary();
    }

    class TypeSafeFasterListForECS<T>: FasterList<T> where T:INode
    {
        protected TypeSafeFasterListForECS()
        {
            _mappedIndices = new Dictionary<int, int>();
        }
        
        public void UnorderedRemove(int mappedIndex)
        {
            var index = _mappedIndices[mappedIndex];
            _mappedIndices.Remove(mappedIndex);

            if (UnorderedRemoveAt(index))
                _mappedIndices[this[index].ID] = index;
        }
        
        public void AddRange(ITypeSafeList nodeListValue)
        {
            var index = this.Count;
            
            AddRange(nodeListValue as FasterList<T>);
            
            for (int i = index; i < Count; ++i)
                _mappedIndices[this[i].ID] = this.Count;
        }

        readonly Dictionary<int, int> _mappedIndices;
    }

    class TypeSafeFasterListForECSForStructs<T> : TypeSafeFasterListForECS<T>, ITypeSafeList where T:struct, INode
    {
        public ITypeSafeList Create()
        {
            return new TypeSafeFasterListForECSForStructs<T>();
        }

        public bool isQueryiableNode
        {
            get { return false; }
        }

        public ITypeSafeDictionary CreateIndexedDictionary()
        {
            throw new Exception("Not Allowed");
        }
    }
    
    class TypeSafeFasterListForECSForClasses<T> : TypeSafeFasterListForECS<T>, ITypeSafeList where T:NodeWithID
    {
        public ITypeSafeList Create()
        {
            return new TypeSafeFasterListForECSForClasses<T>();
        }

        public bool isQueryiableNode
        {
            get { return true; }
        }

        public ITypeSafeDictionary CreateIndexedDictionary()
        {
            return new TypeSafeDictionary<T>();
        }
    }
}
