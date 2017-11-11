using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public class EngineNodeDB : IEngineNodeDB
    {
        internal EngineNodeDB(  Dictionary<Type, ITypeSafeList> nodesDB, 
                                Dictionary<Type, ITypeSafeDictionary> nodesDBdic,
                                Dictionary<Type, ITypeSafeList> metaNodesDB)
        {
            _nodesDB = nodesDB;
            _nodesDBdic = nodesDBdic;
            _metaNodesDB = metaNodesDB;
        }

        public FasterReadOnlyList<T> QueryNodes<T>()
        {
            var type = typeof(T);

            ITypeSafeList nodes;

            if (_nodesDB.TryGetValue(type, out nodes) == false)
                return RetrieveEmptyNodeList<T>();

            return new FasterReadOnlyList<T>((FasterList<T>)nodes);
        }

        public ReadOnlyDictionary<int, T> QueryIndexableNodes<T>()
        {
            var type = typeof(T);

            ITypeSafeDictionary nodes;

            if (_nodesDBdic.TryGetValue(type, out nodes) == false)
                return TypeSafeDictionary<int, T>.Default;

            return new ReadOnlyDictionary<int, T>(nodes as Dictionary<int, T>);
        }

        public T QueryMetaNode<T>(int metaEntityID)
        {
            return QueryNode<T>(metaEntityID);
        }

        public bool TryQueryMetaNode<T>(int metaEntityID, out T node)
        {
            return TryQueryNode(metaEntityID, out node);
        }

        public FasterReadOnlyList<T> QueryMetaNodes<T>() 
        {
            var type = typeof(T);

            ITypeSafeList nodes;

            if (_metaNodesDB.TryGetValue(type, out nodes) == false)
                return RetrieveEmptyNodeList<T>();

            return new FasterReadOnlyList<T>((FasterList<T>)nodes);
        }

        public bool TryQueryNode<T>(int ID, out T node)
        {
            var type = typeof(T);

            T internalNode;

            ITypeSafeDictionary nodes;

            if (_nodesDBdic.TryGetValue(type, out nodes) &&
                (nodes as Dictionary<int, T>).TryGetValue(ID, out internalNode))
            {
                node = internalNode;

                return true;
            }
            
            node = default(T);

            return false;
        }

        public T QueryNode<T>(int ID)
        {
            var type = typeof(T);

            T internalNode; ITypeSafeDictionary nodes;

            if (_nodesDBdic.TryGetValue(type, out nodes) &&
                (nodes as Dictionary<int, T>).TryGetValue(ID, out internalNode))
                return (T)internalNode;

            throw new Exception("Node Not Found");
        }

        static FasterReadOnlyList<T> RetrieveEmptyNodeList<T>()
        {
            return FasterReadOnlyList<T>.DefaultList;
        }

        readonly Dictionary<Type, ITypeSafeList>              _nodesDB;
        readonly Dictionary<Type, ITypeSafeDictionary>        _nodesDBdic;
        readonly Dictionary<Type, ITypeSafeList>              _metaNodesDB;
    }
}
