using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public class StructNodes<T>: IStructNodes where T:struct, IStructNodeWithID
    {
        public FasterList<T> list
        {
            get
            {
                return _internalList;
            }
        }

        public StructNodes()
        {
            _internalList = new FasterList<T>();
        }

        public void Add(T node)
        {
            T convert = (T)node;

            _internalList.Add(convert);
        }

        readonly FasterList<T> _internalList;
    }

    public class StructGroupNodes<T>: IStructGroupNodes
        where T : struct, IGroupedStructNodeWithID
    {
        public void Add(int groupID, T node)
        {
            T convert = (T)node;

            var fasterList = GetList(groupID);
            _indices[node.ID] = fasterList.Count;

            fasterList.Add(convert);
        }

        public void Remove(int groupID, T node)
        {
            var fasterList = GetList(groupID);
            var index = _indices[node.ID];
            _indices.Remove(node.ID);

            if (fasterList.UnorderedRemoveAt(index))
                _indices[fasterList[index].ID] = index;
        }

        public FasterList<T> GetList(int groupID)
        {
            return _nodes[groupID];
        }

        readonly Dictionary<int, int> _indices = new Dictionary<int, int>();
        Dictionary<int, FasterList<T>> _nodes = new Dictionary<int, FasterList<T>>();
    }
}

namespace Svelto.ECS.Internal
{
    public interface IStructGroupNodes
    {
    }

    public interface IStructNodes
    {
    }
}