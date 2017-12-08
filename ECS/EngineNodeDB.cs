using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public class EngineNodeDB : IEngineNodeDB
    {
        internal EngineNodeDB(  Dictionary<Type, ITypeSafeList> nodesDB, 
                                Dictionary<Type, ITypeSafeDictionary> nodesDBdic,
                                Dictionary<Type, ITypeSafeList> metaNodesDB,
                                Dictionary<int, Dictionary<Type, ITypeSafeList>>  groupNodesDB)
        {
            _nodesDB = nodesDB;
            _nodesDBdic = nodesDBdic;
            _metaNodesDB = metaNodesDB;
            _groupNodesDB = groupNodesDB;
        }

        public FasterReadOnlyList<T> QueryNodes<T>()
        {
            var type = typeof(T);

            ITypeSafeList nodes;

            if (_nodesDB.TryGetValue(type, out nodes) == false)
                return RetrieveEmptyNodeList<T>();

            return new FasterReadOnlyList<T>((FasterList<T>)nodes);
        }

        public FasterReadOnlyList<T> QueryGroupedNodes<T>(int @group)
        {
            return new FasterReadOnlyList<T>(_groupNodesDB[group] as FasterList<T>);
        }

        public T[] QueryNodesAsArray<T>(out int count) where T : struct
        {
            var type = typeof(T);
            count = 0;
            
            ITypeSafeList nodes;

            if (_nodesDB.TryGetValue(type, out nodes) == false)
                return null;
            
            var castedNodes = (FasterList<T>)nodes;

            count = castedNodes.Count;

            return castedNodes.ToArrayFast();
        }

        public ReadOnlyDictionary<int, T> QueryIndexableNodes<T>() where T:NodeWithID
        {
            var type = typeof(T);

            ITypeSafeDictionary nodes;

            if (_nodesDBdic.TryGetValue(type, out nodes) == false)
                return TypeSafeDictionary<T>.Default;

            return new ReadOnlyDictionary<int, T>(nodes as Dictionary<int, T>);
        }

        public T QueryMetaNode<T>(int metaEntityID) where T:NodeWithID
        {
            return QueryNode<T>(metaEntityID);
        }

        public bool TryQueryMetaNode<T>(int metaEntityID, out T node) where T:NodeWithID
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

        public bool TryQueryNode<T>(int ID, out T node) where T:NodeWithID
        {
            var type = typeof(T);

            T internalNode;

            ITypeSafeDictionary nodes;
            TypeSafeDictionary<T> casted;

            _nodesDBdic.TryGetValue(type, out nodes);
            casted = nodes as TypeSafeDictionary<T>;

            if (casted != null &&
                casted.TryGetValue(ID, out internalNode))
            {
                node = (T) internalNode;

                return true;
            }
            
            node = default(T);

            return false;
        }

        public T QueryNode<T>(int ID) where T:NodeWithID
        {
            var type = typeof(T);

            T internalNode; ITypeSafeDictionary nodes;
            TypeSafeDictionary<T> casted;

            _nodesDBdic.TryGetValue(type, out nodes);
            casted = nodes as TypeSafeDictionary<T>;

            if (casted != null &&
                casted.TryGetValue(ID, out internalNode))
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
        readonly Dictionary<int, Dictionary<Type, ITypeSafeList>> _groupNodesDB;
    }
}
