using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.DataStructures;
using UnityEngine;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS
{
    class Scheduler : MonoBehaviour
    {
        IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                OnTick();
            }
        }

        internal Action OnTick;
    }

    public sealed class EnginesRoot : IEnginesRoot, IEntityFactory
    {
        public EnginesRoot()
        {
            _nodeEngines = new Dictionary<Type, FasterList<INodeEngine<INode>>>();
            _engineRootWeakReference = new WeakReference<EnginesRoot>(this);
            _otherEnginesReferences = new FasterList<IEngine>();

            _nodesDB = new Dictionary<Type, FasterList<INode>>();
            _nodesDBdic = new Dictionary<Type, Dictionary<int, INode>>();

            _nodesToAdd = new FasterList<INode>();
            _groupNodesToAdd = new FasterList<INode>();

            _nodesDBgroups = new Dictionary<Type, FasterList<INode>>();

            GameObject go = new GameObject("ECSScheduler");

            go.AddComponent<Scheduler>().OnTick += SubmitNodes;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
            GameObject debugEngineObject = new GameObject("Engine Debugger");
            debugEngineObject.gameObject.AddComponent<EngineProfilerBehaviour>();
#endif
        }

        void SubmitNodes()
        {
            int groupNodesCount;
            int nodesCount;
            bool newNodesHaveBeenAddedWhileIterating;
            int startNodes = 0;
            int startGroupNodes = 0;
            int numberOfReenteringLoops = 0;

            do
            {
                groupNodesCount = _groupNodesToAdd.Count;
                nodesCount = _nodesToAdd.Count;

                for (int i = startNodes; i < nodesCount; i++)
                {
                    var node = _nodesToAdd[i];
                    AddNodeToTheDB(node, node.GetType());
                }

                for (int i = startGroupNodes; i < groupNodesCount; i++)
                {
                    var node = _groupNodesToAdd[i];
                    AddNodeToGroupDB(node, node.GetType());
                }

                for (int i = startNodes; i < nodesCount; i++)
                {
                    var node = _nodesToAdd[i];
                    AddNodeToTheSuitableEngines(node, node.GetType());
                }

                for (int i = startGroupNodes; i < groupNodesCount; i++)
                {
                    var node = _groupNodesToAdd[i];
                    AddNodeToTheSuitableEngines(node, node.GetType());
                }

                newNodesHaveBeenAddedWhileIterating = _groupNodesToAdd.Count > groupNodesCount || _nodesToAdd.Count > nodesCount;

                startNodes = nodesCount;
                startGroupNodes = groupNodesCount;

                if (numberOfReenteringLoops > 5)
                    throw new Exception("possible infinite loop found creating Entities inside INodesEngine Add method, please consider building entities outside INodesEngine Add method");

                 numberOfReenteringLoops++;

            } while (newNodesHaveBeenAddedWhileIterating);

            _nodesToAdd.Clear();
            _groupNodesToAdd.Clear();
        }

        public void AddEngine(IEngine engine)
        {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
            EngineProfiler.AddEngine(engine);
#endif
            if (engine is IQueryableNodeEngine)
                (engine as IQueryableNodeEngine).nodesDB = new EngineNodeDB(_nodesDB, _nodesDBdic, _nodesDBgroups);

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
                    InternalRemove(node);
            });
            
            _nodesToAdd.AddRange(entityNodes);
        }

        public void BuildEntityGroup(int groupID, EntityDescriptor ed)
        {
            var entityNodes = ed.BuildNodes(groupID, (node) =>
            {
                if (_engineRootWeakReference.IsValid == true)
                    InternalGroupRemove(node);
            });

            _groupNodesToAdd.AddRange(entityNodes);
        }

        static void AddEngine<T>(T engine, Type[] types, Dictionary<Type, FasterList<INodeEngine<INode>>> engines) where T : INodeEngine<INode>
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

        void AddNodeToGroupDB(INode node, Type nodeType)
        {
            FasterList<INode> nodes;
            if (_nodesDBgroups.TryGetValue(nodeType, out nodes) == false)
                nodes = _nodesDBgroups[nodeType] = new FasterList<INode>();

            nodes.Add(node);

            AddNodeToNodesDictionary(node, nodeType);
        }

        void AddNodeToTheDB<T>(T node, Type nodeType) where T : INode
        {
            FasterList<INode> nodes;
            if (_nodesDB.TryGetValue(nodeType, out nodes) == false)
                nodes = _nodesDB[nodeType] = new FasterList<INode>();

            nodes.Add(node);

            AddNodeToNodesDictionary(node, nodeType);
        }

        void AddNodeToNodesDictionary<T>(T node, Type nodeType) where T : INode
        {
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
            {
                for (int j = 0; j < enginesForNode.Count; j++)
                {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
                    EngineProfiler.MonitorAddDuration(AddNodeToEngine, enginesForNode[j], node);
#else 
                    enginesForNode[j].Add(node);
#endif
                }
            }
        }

        void RemoveNodeFromTheDB<T>(T node, Type nodeType) where T : INode
        {
            FasterList<INode> nodes;
            if (_nodesDB.TryGetValue(nodeType, out nodes) == true)
                nodes.UnorderredRemove(node); //should I remove it from the dictionary if length is zero?

            RemoveNodeFromNodesDictionary(node, nodeType);
        }

        void RemoveNodeFromNodesDictionary<T>(T node, Type nodeType) where T : INode
        {
            if (node is NodeWithID)
            {
                Dictionary<int, INode> nodesDic;

                if (_nodesDBdic.TryGetValue(nodeType, out nodesDic))
                    nodesDic.Remove((node as NodeWithID).ID);
            }
        }

        void RemoveNodeFromGroupDB<T>(T node, Type nodeType) where T : INode
        {
            FasterList<INode> nodes;
            if (_nodesDBgroups.TryGetValue(nodeType, out nodes) == true)
                nodes.UnorderredRemove(node); //should I remove it from the dictionary if length is zero?

            RemoveNodeFromNodesDictionary(node, nodeType);
        }

        void RemoveNodeFromEngines<T>(T node, Type nodeType) where T : INode
        {
            FasterList<INodeEngine<INode>> enginesForNode;

            if (_nodeEngines.TryGetValue(nodeType, out enginesForNode))
            {
                for (int j = 0; j < enginesForNode.Count; j++)
                {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
                    EngineProfiler.MonitorRemoveDuration(RemoveNodeFromEngine, enginesForNode[j], node);
#else 
                    enginesForNode[j].Remove(node);
#endif
                }
            }
        }
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
        void AddNodeToEngine(INodeEngine<INode> engine, INode node)
        {
            engine.Add(node);
        }

        void RemoveNodeFromEngine(INodeEngine<INode> engine, INode node)
        {
            engine.Remove(node);
        }
#endif
        
        void InternalRemove<T>(T node) where T : INode
        {
            Type nodeType = node.GetType();

            RemoveNodeFromEngines(node, nodeType);
            RemoveNodeFromTheDB(node, node.GetType());
        }

        void InternalGroupRemove<T>(T node) where T : INode
        {
            Type nodeType = node.GetType();

            RemoveNodeFromEngines(node, nodeType);
            RemoveNodeFromGroupDB(node, node.GetType());
        }

        Dictionary<Type, FasterList<INodeEngine<INode>>>     _nodeEngines;
        FasterList<IEngine>                                  _otherEnginesReferences;

        Dictionary<Type, FasterList<INode>>                  _nodesDB;
        Dictionary<Type, Dictionary<int, INode>>             _nodesDBdic;

        Dictionary<Type, FasterList<INode>>                  _nodesDBgroups;

        FasterList<INode>                                    _nodesToAdd;
        FasterList<INode>                                    _groupNodesToAdd;

        WeakReference<EnginesRoot>                           _engineRootWeakReference;

        //integrated pooling system
        //add debug panel like Entitas has
        //GCHandle should be used to reduce the number of strong references
        //datastructure could be thread safe

        //future enhancements:
    }
}

