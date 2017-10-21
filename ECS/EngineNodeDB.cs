using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public class EngineNodeDB : IEngineNodeDB
    {
        internal EngineNodeDB(  Dictionary<Type, FasterList<INode>> nodesDB, 
                                Dictionary<Type, Dictionary<int, INode>> nodesDBdic,
                                Dictionary<Type, FasterList<INode>> metaNodesDB)
        {
            _nodesDB = nodesDB;
            _nodesDBdic = nodesDBdic;
            _metaNodesDB = metaNodesDB;
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

        public T QueryMetaNode<T>(int metaEntityID) where T : INode 
        {
            return QueryNode<T>(metaEntityID);
        }

        public bool TryQueryMetaNode<T>(int metaEntityID, out T node) where T : INode
        {
            return TryQueryNode(metaEntityID, out node);
        }

        public FasterReadOnlyListCast<INode, T> QueryMetaNodes<T>() where T : INode
        {
            var type = typeof(T);

            if (_metaNodesDB.ContainsKey(type) == false)
                return RetrieveEmptyNodeList<T>();

            return new FasterReadOnlyListCast<INode, T>(_metaNodesDB[type]);
        }

        public bool TryQueryNode<T>(int ID, out T node) where T:INode
        {
            var type = typeof(T);

            INode internalNode;

            if (_nodesDBdic.ContainsKey(type) && 
                _nodesDBdic[type].TryGetValue(ID, out internalNode))
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

            if (_nodesDBdic.ContainsKey(type) && 
                _nodesDBdic[type].TryGetValue(ID, out internalNode))
                return (T)internalNode;

            throw new Exception("Node Not Found");
        }

        public FasterReadOnlyListCast<INode, T> QueryGroupNodes<T>(int groupID) where T : INode
        {
            var type = typeof(T);

            if (_nodesDB.ContainsKey(type) == false)
                return RetrieveEmptyNodeList<T>();

            return new FasterReadOnlyListCast<INode, T>(_nodesDB[type]);
        }

        static FasterReadOnlyListCast<INode, T> RetrieveEmptyNodeList<T>() where T : INode
        {
            return FasterReadOnlyListCast<INode, T>.DefaultList;
        }

        readonly Dictionary<Type, FasterList<INode>>      _nodesDB;
        readonly Dictionary<Type, Dictionary<int, INode>> _nodesDBdic;
        readonly Dictionary<Type, FasterList<INode>>      _metaNodesDB;

        readonly ReadOnlyDictionary<int, INode> _defaultEmptyNodeDict = new ReadOnlyDictionary<int, INode>(new Dictionary<int, INode>());
    }
}
