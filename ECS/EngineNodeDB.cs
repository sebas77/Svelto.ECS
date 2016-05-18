using System;
using System.Collections.Generic;
using Svelto.DataStructures;

namespace Svelto.ES
{
    class EngineNodeDB : IEngineNodeDB
    {
        internal EngineNodeDB(Dictionary<Type, FasterList<INode>> nodesDB, Dictionary<Type, Dictionary<int, INode>> nodesDBdic)
        {
            this._nodesDB = nodesDB;
            this._nodesDBdic = nodesDBdic;
        }

        public FasterReadOnlyList<INode> QueryNodes<T>() where T:INode
        {
            var type = typeof(T);

            if (_nodesDB.ContainsKey(type) == false)
                return _defaultEmptyNodeList;

            return new FasterReadOnlyList<INode>(_nodesDB[type]);
        }

        public ReadOnlyDictionary<int, INode> QueryIndexableNodes<T>() where T:INode
        {
            var type = typeof(T);

            if (_nodesDBdic.ContainsKey(type) == false)
                return _defaultEmptyNodeDict;

            return new ReadOnlyDictionary<int, INode>(_nodesDBdic[type]);
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

        Dictionary<Type, FasterList<INode>>      _nodesDB;
        Dictionary<Type, Dictionary<int, INode>> _nodesDBdic;

        FasterReadOnlyList<INode>           _defaultEmptyNodeList = new FasterReadOnlyList<INode>(new FasterList<INode>());
        ReadOnlyDictionary<int, INode>      _defaultEmptyNodeDict = new ReadOnlyDictionary<int, INode>(new Dictionary<int, INode>());
    }
}
