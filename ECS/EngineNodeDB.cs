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
            _nodesDB = new DataStructures.WeakReference<Dictionary<Type, FasterList<INode>>>(nodesDB);
            _nodesDBdic = new DataStructures.WeakReference<Dictionary<Type, Dictionary<int, INode>>>(nodesDBdic);
            _nodesDBgroups = new DataStructures.WeakReference<Dictionary<Type, FasterList<INode>>>(nodesDBgroups);
        }

        public FasterReadOnlyListCast<INode, T> QueryNodes<T>() where T:INode
        {
            var type = typeof(T);

            if (_nodesDB.IsValid == false || _nodesDB.Target.ContainsKey(type) == false)
                return RetrieveEmptyNodeList<T>();

            return new FasterReadOnlyListCast<INode, T>(_nodesDB.Target[type]);
        }

    /*    public FasterReadOnlyList<T> QueryStructNodes<T>() where T:struct
        {
            var type = typeof(T);

            if (_nodesDBStructs.ContainsKey(type) == false)
                return RetrieveEmptyStructNodeList<T>();

            return new FasterReadOnlyList<T>(((StructNodeList<T>)(_nodesDBStructs[type])).list);
        }*/

        public ReadOnlyDictionary<int, INode> QueryIndexableNodes<T>() where T:INode
        {
            var type = typeof(T);

            if (_nodesDB.IsValid == false  || _nodesDBdic.Target.ContainsKey(type) == false)
                return _defaultEmptyNodeDict;

            return new ReadOnlyDictionary<int, INode>(_nodesDBdic.Target[type]);
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

            if (_nodesDBgroups.IsValid == false  || _nodesDBgroups.Target.ContainsKey(type) == false)
                return RetrieveEmptyNodeList<T>();

            return new FasterReadOnlyListCast<INode, T>(_nodesDBgroups.Target[type]);
        }

        public bool QueryNode<T>(int ID, out T node) where T:INode
        {
            var type = typeof(T);

            INode internalNode;

            if (_nodesDBdic.IsValid && _nodesDBdic.Target.ContainsKey(type) && _nodesDBdic.Target[type].TryGetValue(ID, out internalNode))
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

            if (_nodesDBdic.IsValid && _nodesDBdic.Target.ContainsKey(type) && _nodesDBdic.Target[type].TryGetValue(ID, out internalNode))
                return (T)internalNode;

            throw new Exception("Node Not Found");
        }

        static FasterReadOnlyListCast<INode, T> RetrieveEmptyNodeList<T>() where T : INode
        {
            return new FasterReadOnlyListCast<INode, T>(FasterList<INode>.DefaultList);
        }

        static FasterReadOnlyList<T> RetrieveEmptyStructNodeList<T>() where T : struct
        {
            return new FasterReadOnlyList<T>(FasterList<T>.DefaultList);
        }

        Svelto.DataStructures.WeakReference<Dictionary<Type, FasterList<INode>>>      _nodesDB;
        Svelto.DataStructures.WeakReference<Dictionary<Type, Dictionary<int, INode>>> _nodesDBdic;
        Svelto.DataStructures.WeakReference<Dictionary<Type, FasterList<INode>>>      _nodesDBgroups;
//        Dictionary<Type, StructNodeList>         _nodesDBStructs;

        //Dictionary<Type, ThreadSafeFasterList<INode>>       _nodesDB;
        //Dictionary<Type, ThreadsSafeDictionary<int, INode>> _nodesDBdic;
//        Dictionary<Type, ThreadSafeFasterList<INode>>       _nodesDBgroups;

        ReadOnlyDictionary<int, INode> _defaultEmptyNodeDict = new ReadOnlyDictionary<int, INode>(new Dictionary<int, INode>());

        class StructNodeList
        { }

        class StructNodeList<T> : StructNodeList where T : struct
        {
            public FasterList<T> list = new FasterList<T>();
        }
    }
}
