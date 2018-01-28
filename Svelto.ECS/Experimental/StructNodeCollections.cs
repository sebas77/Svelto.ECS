#if EXPERIMENTAL
using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Experimental.Internal;

namespace Svelto.ECS.Experimental.Internal
{
    public interface IStructEntityViewEngine : IEngine
    {
        void CreateStructEntityViews(SharedStructEntityViewLists sharedStructEntityViewLists);
    }

    public interface IGroupedStructEntityViewsEngine : IEngine
    {
        void CreateStructEntityViews(SharedGroupedStructEntityViewsLists sharedStructEntityViewLists);
    }
}

namespace Svelto.ECS.Experimental
{
    public interface IGroupedEntityView
    {
        int groupID { get; set; }
    }
    
    /// <summary>
    /// The engines can receive and store IEntityViews structs
    /// Unboxing will happen during the Add, but the 
    /// data will then be stored and processed as stucts
    /// </summary>
    public interface IStructEntityViewEngine<T> : IStructEntityViewEngine where T:struct, IEntityStruct
    { }

    /// <summary>
    /// same as above, but the entityViews are grouped by ID
    /// usually the ID is the owner of the entityViews of that
    /// group
    /// </summary>
    public interface IGroupedStructEntityViewsEngine<T> : IGroupedStructEntityViewsEngine where T : struct, IGroupedEntityView
    {
        void Add(ref T entityView);
        void Remove(ref T entityView);
    }
        
    public sealed class StructEntityViews<T> where T:struct, IEntityStruct
    {
        public T[] GetList(out int numberOfItems)
        {
            numberOfItems = _internalList.Count;
            return _internalList.ToArrayFast();
        }

        public StructEntityViews(SharedStructEntityViewLists container)
        {
            _internalList = SharedStructEntityViewLists.NoVirt.GetList<T>(container);
        }

        public void Add(T entityView)
        {
            T convert = (T)entityView;

            _internalList.Add(convert);
        }

        readonly FasterList<T> _internalList;
    }

    public struct StructGroupEntityViews<T>
        where T : struct, IEntityView
    {
        public StructGroupEntityViews(SharedGroupedStructEntityViewsLists container)
        {
            _container = container;
            indices = new Dictionary<int, int>();
        }

        public void Add(int groupID, T entityView)
        {
            T convert = (T)entityView;

            var fasterList = (SharedGroupedStructEntityViewsLists.NoVirt.GetList<T>(_container, groupID) as FasterList<T>);
            indices[entityView.ID] = fasterList.Count;

            fasterList.Add(convert);
        }

        public void Remove(int groupID, T entityView)
        {
            var fasterList = (SharedGroupedStructEntityViewsLists.NoVirt.GetList<T>(_container, groupID) as FasterList<T>);
            var index = indices[entityView.ID];
            indices.Remove(entityView.ID);

            if (fasterList.UnorderedRemoveAt(index))
                indices[fasterList[index].ID] = index;
        }

        public T[] GetList(int groupID, out int numberOfItems)
        {
            var fasterList = (SharedGroupedStructEntityViewsLists.NoVirt.GetList<T>(_container, groupID) as FasterList<T>);
            
            return FasterList<T>.NoVirt.ToArrayFast(fasterList, out numberOfItems);
        }

        readonly SharedGroupedStructEntityViewsLists  _container;
        readonly Dictionary<int, int> indices;
    }

    public class SharedStructEntityViewLists
    {
        internal SharedStructEntityViewLists()
        {
            _collection = new Dictionary<Type, IFasterList>();
        }

        internal static class NoVirt
        {
            internal static FasterList<T> GetList<T>(SharedStructEntityViewLists obj) where T : struct
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

    public class SharedGroupedStructEntityViewsLists
    {
        internal SharedGroupedStructEntityViewsLists()
        {
            _collection = new Dictionary<Type, Dictionary<int, IFasterList>>();
        }

        internal static class NoVirt
        {
            internal static IFasterList GetList<T>(SharedGroupedStructEntityViewsLists list, int groupID) where T : struct
            {
                Dictionary<int, IFasterList> dic = GetGroup<T>(list);
                IFasterList localList;

                if (dic.TryGetValue(groupID, out localList))
                    return localList;

                localList = new FasterList<T>();
                dic.Add(groupID, localList);

                return localList;
            }

            internal static Dictionary<int, IFasterList> GetGroup<T>(SharedGroupedStructEntityViewsLists list) where T : struct
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
#endif