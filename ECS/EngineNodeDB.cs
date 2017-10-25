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

            FasterList<INode> nodes;

            if (_nodesDB.TryGetValue(type, out nodes) == false)
                return RetrieveEmptyNodeList<T>();

            return new FasterReadOnlyListCast<INode, T>(nodes);
        }

        public ReadOnlyDictionary<int, INode> QueryIndexableNodes<T>() where T:INode
        {
            var type = typeof(T);

            Dictionary<int, INode> nodes;

            if (_nodesDBdic.TryGetValue(type, out nodes) == false)
                return _defaultEmptyNodeDict;

            return new ReadOnlyDictionary<int, INode>(nodes);
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

            FasterList<INode> nodes;

            if (_metaNodesDB.TryGetValue(type, out nodes) == false)
                return RetrieveEmptyNodeList<T>();

            return new FasterReadOnlyListCast<INode, T>(nodes);
        }

        public bool TryQueryNode<T>(int ID, out T node) where T:INode
        {
            var type = typeof(T);

            INode internalNode;

            Dictionary<int, INode> nodes;

            if (_nodesDBdic.TryGetValue(type, out nodes) &&
                nodes.TryGetValue(ID, out internalNode))
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

            INode internalNode; Dictionary<int, INode> nodes;

            if (_nodesDBdic.TryGetValue(type, out nodes) &&
                nodes.TryGetValue(ID, out internalNode))
                return (T)internalNode;

            throw new Exception("Node Not Found");
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
