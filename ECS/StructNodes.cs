using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public class StructNodes<T> where T:struct, IStructNodeWithID
    {
        public T[] GetList(out int numberOfItems)
        {
            numberOfItems = _internalList.Count;
            return _internalList.ToArrayFast();
        }

        public StructNodes(SharedStructNodeLists container)
        {
            _internalList = container.GetList<T>();
        }

        public void Add(T node)
        {
            T convert = (T)node;

            _internalList.Add(convert);
        }

        readonly FasterList<T> _internalList;
    }

    public class StructGroupNodes<T>
        where T : struct, IGroupedStructNodeWithID
    {
        public StructGroupNodes(SharedGroupedStructNodesLists container)
        {
            _container = container;
        }

        public void Add(int groupID, T node)
        {
            T convert = (T)node;

            var fasterList = (_container.GetList<T>(groupID) as FasterList<T>);
            indices[node.ID] = fasterList.Count;

            fasterList.Add(convert);
        }

        public void Remove(int groupID, T node)
        {
            var fasterList = (_container.GetList<T>(groupID) as FasterList<T>);
            var index = indices[node.ID];
            indices.Remove(node.ID);

            if (fasterList.UnorderedRemoveAt(index))
                indices[fasterList[index].ID] = index;
        }

        public T[] GetList(int groupID, out int numberOfItems)
        {
            var fasterList = (_container.GetList<T>(groupID) as FasterList<T>);
            numberOfItems = fasterList.Count;
            return fasterList.ToArrayFast();
        }

        readonly SharedGroupedStructNodesLists  _container;
        readonly Dictionary<int, int> indices = new Dictionary<int, int>();
    }

    public class SharedStructNodeLists
    {
        readonly Dictionary<Type, IFasterList> _collection;

        internal SharedStructNodeLists()
        {
            _collection = new Dictionary<Type, IFasterList>();
        }

        internal FasterList<T> GetList<T>() where T:struct
        {
            IFasterList list;
            if (_collection.TryGetValue(typeof (T), out list))
            {
                return list as FasterList<T>;
            }

            list = new FasterList<T>();

            _collection.Add(typeof (T), list);

            return (FasterList<T>) list;
        }
    }

    public class SharedGroupedStructNodesLists
    {
        internal SharedGroupedStructNodesLists()
        {
            _collection = new Dictionary<Type, Dictionary<int, IFasterList>>();
        }

        internal IFasterList GetList<T>(int groupID) where T : struct
        {
            Dictionary<int, IFasterList> dic = GetGroup<T>();
            IFasterList localList;

            if (dic.TryGetValue(groupID, out localList))
                return localList;

            localList = new FasterList<T>();
            dic.Add(groupID, localList);

            return localList;
        }

        internal Dictionary<int, IFasterList> GetGroup<T>() where T : struct
        {
            Dictionary<int, IFasterList> dic;

            if (_collection.TryGetValue(typeof(T), out dic))
            {
                return dic;
            }

            dic = new Dictionary<int, IFasterList>();

            _collection.Add(typeof(T), dic);

            return dic;
        }

        readonly Dictionary<Type, Dictionary<int, IFasterList>> _collection;
    }
}