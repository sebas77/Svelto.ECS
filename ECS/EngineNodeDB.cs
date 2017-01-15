using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    class EngineNodeDB : IEngineNodeDB
    {
        internal EngineNodeDB(  Dictionary<Type, FasterList<INode>> nodesDB, 
                                Dictionary<Type, Dictionary<int, INode>> nodesDBdic,
                                Dictionary<Type, FasterList<INode>> nodesDBgroups)
        {
            _nodesDB = nodesDB;
            _nodesDBdic = nodesDBdic;
            _nodesDBgroups = nodesDBgroups;
        }

        public FasterReadOnlyListCast<INode, T> QueryNodes<T>() where T:INode
        {
            var type = typeof(T);

            if (_nodesDB.ContainsKey(type) == false)
                return RetrieveEmptyNodeList<T>();

            return new FasterReadOnlyListCast<INode, T>(_nodesDB[type]);
        }

        public ReadOnlyDictionary<int, INode> QueryIndexableNodes<T>() where T:INode
        {
            var type = typeof(T);

            if (_nodesDBdic.ContainsKey(type) == false)
                return _defaultEmptyNodeDict;

            return new ReadOnlyDictionary<int, INode>(_nodesDBdic[type]);
        }

        public T QueryNodeFromGroup<T>(int groupID) where T : INode 
        {
            return QueryNode<T>(groupID);
        }

        public bool QueryNodeFromGroup<T>(int groupID, out T node) where T : INode
        {
            return QueryNode<T>(groupID, out node);
        }

        public FasterReadOnlyListCast<INode, T> QueryNodesFromGroups<T>() where T : INode
        {
            var type = typeof(T);

            if (_nodesDBgroups.ContainsKey(type) == false)
                return RetrieveEmptyNodeList<T>();

            return new FasterReadOnlyListCast<INode, T>(_nodesDBgroups[type]);
        }

        public bool QueryNode<T>(int ID, out T node) where T:INode
        {
            var type = typeof(T);

            INode internalNode;

            if (_nodesDBdic.ContainsKey(type) && _nodesDBdic[type].TryGetValue(ID, out internalNode))
            {
                node = (T)internalNode;

                return true;
            }

            node = default(T);

            return false;
        }

        public T QueryNode<T>(int ID) where T:INode
        {
            var type = typeof(T);

            INode internalNode;

            if (_nodesDBdic.ContainsKey(type) && _nodesDBdic[type].TryGetValue(ID, out internalNode))
                return (T)internalNode;

            throw new Exception("Node Not Found");
        }

        static FasterReadOnlyListCast<INode, T> RetrieveEmptyNodeList<T>() where T : INode
        {
            return new FasterReadOnlyListCast<INode, T>(FasterReadOnlyListCast<INode, T>.DefaultList);
        }

        Dictionary<Type, FasterList<INode>>      _nodesDB;
        Dictionary<Type, Dictionary<int, INode>> _nodesDBdic;
        Dictionary<Type, FasterList<INode>>      _nodesDBgroups;

        ReadOnlyDictionary<int, INode> _defaultEmptyNodeDict = new ReadOnlyDictionary<int, INode>(new Dictionary<int, INode>());
    }
}
