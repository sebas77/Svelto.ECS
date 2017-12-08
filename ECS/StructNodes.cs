using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public sealed class StructNodes<T> where T:struct, IStructNodeWithID
    {
        public T[] GetList(out int numberOfItems)
        {
            numberOfItems = _internalList.Count;
            return _internalList.ToArrayFast();
        }

        public StructNodes(SharedStructNodeLists container)
        {
            _internalList = SharedStructNodeLists.NoVirt.GetList<T>(container);
        }

        public void Add(T node)
        {
            T convert = (T)node;

            _internalList.Add(convert);
        }

        readonly FasterList<T> _internalList;
    }

    public struct StructGroupNodes<T>
        where T : struct, IStructNodeWithID
    {
        public StructGroupNodes(SharedGroupedStructNodesLists container)
        {
            _container = container;
            indices = new Dictionary<int, int>();
        }

        public void Add(int groupID, T node)
        {
            T convert = (T)node;

            var fasterList = (SharedGroupedStructNodesLists.NoVirt.GetList<T>(_container, groupID) as FasterList<T>);
            indices[node.ID] = fasterList.Count;

            fasterList.Add(convert);
        }

        public void Remove(int groupID, T node)
        {
            var fasterList = (SharedGroupedStructNodesLists.NoVirt.GetList<T>(_container, groupID) as FasterList<T>);
            var index = indices[node.ID];
            indices.Remove(node.ID);

            if (fasterList.UnorderedRemoveAt(index))
                indices[fasterList[index].ID] = index;
        }

        public T[] GetList(int groupID, out int numberOfItems)
        {
            var fasterList = (SharedGroupedStructNodesLists.NoVirt.GetList<T>(_container, groupID) as FasterList<T>);
            
            return FasterList<T>.NoVirt.ToArrayFast(fasterList, out numberOfItems);
        }

        readonly SharedGroupedStructNodesLists  _container;
        readonly Dictionary<int, int> indices;
    }

    public class SharedStructNodeLists
    {
        internal SharedStructNodeLists()
        {
            _collection = new Dictionary<Type, IFasterList>();
        }

        internal static class NoVirt
        {
            internal static FasterList<T> GetList<T>(SharedStructNodeLists obj) where T : struct
            {
                IFasterList list;
                if (obj._collection.TryGetValue(typeof(T), out list))
                {
                    return list as FasterList<T>;
                }

                list = new FasterList<T>();

                obj._collection.Add(typeof(T), list);

                return (FasterList<T>)list;
            }
        }

        readonly Dictionary<Type, IFasterList> _collection;
    }

    public class SharedGroupedStructNodesLists
    {
        internal SharedGroupedStructNodesLists()
        {
            _collection = new Dictionary<Type, Dictionary<int, IFasterList>>();
        }

        internal static class NoVirt
        {
            internal static IFasterList GetList<T>(SharedGroupedStructNodesLists list, int groupID) where T : struct
            {
                Dictionary<int, IFasterList> dic = GetGroup<T>(list);
                IFasterList localList;

                if (dic.TryGetValue(groupID, out localList))
                    return localList;

                localList = new FasterList<T>();
                dic.Add(groupID, localList);

                return localList;
            }

            internal static Dictionary<int, IFasterList> GetGroup<T>(SharedGroupedStructNodesLists list) where T : struct
            {
                Dictionary<int, IFasterList> dic;

                if (list._collection.TryGetValue(typeof(T), out dic))
                {
                    return dic;
                }

                dic = new Dictionary<int, IFasterList>();

                list._collection.Add(typeof(T), dic);

                return dic;
            }
        }

        readonly Dictionary<Type, Dictionary<int, IFasterList>> _collection;
    }
}