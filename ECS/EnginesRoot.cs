using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.Ticker;

namespace Svelto.ES
{
    public sealed class EnginesRoot: IEnginesRoot, IEndOfFrameTickable, IEntityFactory
    {
        public EnginesRoot(ITicker ticker)
        {
            ticker.Add(this);

            _nodeEngines = new Dictionary<Type, FasterList<INodeEngine<INode>>>();
            _engineRootWeakReference = new WeakReference<EnginesRoot>(this);
            _otherEnginesReferences = new FasterList<IEngine>();
            
            _nodesDB = new Dictionary<Type, FasterList<INode>>();
            _nodesDBdic = new Dictionary<Type, Dictionary<int, INode>>();

            _nodesToAdd = new Queue<INode>();
            _nodesToRemove = new Queue<INode>();
        }

        public void EndOfFrameTick(float deltaSec)
        {
            while (_nodesToAdd.Count > 0) InternalAdd(_nodesToAdd.Dequeue());
            while (_nodesToRemove.Count > 0) InternalRemove(_nodesToRemove.Dequeue());
        }

        public void AddEngine(IEngine engine)
        {
            if (engine is IQueryableNodeEngine)
               (engine as IQueryableNodeEngine).nodesDB = new EngineNodeDB(_nodesDB, _nodesDBdic);

            if (engine is INodesEngine)
            {
                var nodesEngine = engine as INodesEngine;

                AddEngine(nodesEngine, nodesEngine.AcceptedNodes(), _nodeEngines);

                return;
            }

            var baseType = engine.GetType().BaseType;
            if (baseType.IsGenericType)
            {
                var genericType = baseType.GetGenericTypeDefinition();
                
                if (genericType == typeof(SingleNodeEngine<>))
                {
                    AddEngine(engine as INodeEngine<INode>, baseType.GetGenericArguments(), _nodeEngines);

                    return;
                }
            }

            _otherEnginesReferences.Add(engine); 
        }

        public void BuildEntity(int ID, EntityDescriptor ed)
        {
            var entityNodes = ed.BuildNodes(ID, (node) =>
            {
                if (_engineRootWeakReference.IsValid == true)
                    _engineRootWeakReference.Target._nodesToRemove.Enqueue(node);
            });
            
            for (int i = 0; i < entityNodes.Count; i++)
                _nodesToAdd.Enqueue(entityNodes[i]);
        }

        static void AddEngine<T>(T engine, Type[] types, Dictionary<Type, FasterList<INodeEngine<INode>>> engines) where T:INodeEngine<INode>
        {
            for (int i = 0; i < types.Length; i++)
            {
                FasterList<INodeEngine<INode>> list;

                var type = types[i];

                if (engines.TryGetValue(type, out list) == false)
                {
                    list = new FasterList<INodeEngine<INode>>();

                    engines.Add(type, list);
                }

                list.Add(engine);
            }
        }

        void InternalAdd<T>(T node) where T:INode
        {
            Type nodeType = node.GetType();

            AddNodeToTheSuitableEngines(node, nodeType);
            AddNodeToTheDB(node, nodeType);
        }

        void InternalRemove<T>(T node) where T:INode
        {
            Type nodeType = node.GetType();

            RemoveNodeFromEngines(node, nodeType);
            RemoveNodeFromTheDB(node, nodeType);
        }

        void AddNodeToTheDB<T>(T node, Type nodeType) where T : INode
        {
            FasterList<INode> nodes;
            if (_nodesDB.TryGetValue(nodeType, out nodes) == false)
                nodes = _nodesDB[nodeType] = new FasterList<INode>();

            nodes.Add(node);

            if (node is NodeWithID)
            {
                Dictionary<int, INode> nodesDic;
                if (_nodesDBdic.TryGetValue(nodeType, out nodesDic) == false)
                    nodesDic = _nodesDBdic[nodeType] = new Dictionary<int, INode>();
            
                nodesDic[(node as NodeWithID).ID] = node;
            }
        }

        void AddNodeToTheSuitableEngines<T>(T node, Type nodeType) where T : INode
        {
            FasterList<INodeEngine<INode>> enginesForNode;

            if (_nodeEngines.TryGetValue(nodeType, out enginesForNode))
                for (int j = 0; j < enginesForNode.Count; j++)
                    enginesForNode[j].Add(node);
        }

        void RemoveNodeFromTheDB<T>(T node, Type nodeType) where T : INode
        {
            FasterList<INode> nodes;
            if (_nodesDB.TryGetValue(nodeType, out nodes) == true)
                nodes.Remove(node); //should I remove it from the dictionary if length is zero?

            if (node is NodeWithID)
            {
                Dictionary<int, INode> nodesDic;

                if (_nodesDBdic.TryGetValue(nodeType, out nodesDic))
                    nodesDic.Remove((node as NodeWithID).ID);
            }
        }

        void RemoveNodeFromEngines<T>(T node, Type nodeType) where T : INode
        {
            FasterList<INodeEngine<INode>> enginesForNode;

            if (_nodeEngines.TryGetValue(nodeType, out enginesForNode))
                for (int j = 0; j < enginesForNode.Count; j++)
                    enginesForNode[j].Remove(node);
        }

        Dictionary<Type, FasterList<INodeEngine<INode>>>     _nodeEngines;
        FasterList<IEngine>                                  _otherEnginesReferences;

        Dictionary<Type, FasterList<INode>>                  _nodesDB;
        Dictionary<Type, Dictionary<int, INode>>             _nodesDBdic;

        Queue<INode>                                         _nodesToAdd;
        Queue<INode>                                         _nodesToRemove;

        WeakReference<EnginesRoot>                           _engineRootWeakReference;
        
        //integrated pooling system
        //add debug panel like Entitas has
        //GCHandle should be used to reduce the number of strong references
        //datastructure could be thread safe

        //future enhancements:
    }
}

