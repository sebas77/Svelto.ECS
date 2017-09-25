using System;
using System.Collections;
using System.Collections.Generic;
using Svelto.DataStructures;
using UnityEngine;
using WeakReference = Svelto.DataStructures.WeakReference<Svelto.ECS.EnginesRoot>;
using Svelto.ECS.NodeSchedulers;
using Svelto.ECS.Internal;
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif
#if NETFX_CORE
using System.Reflection;
#endif

namespace Svelto.ECS
{
    public sealed class EnginesRoot : IEnginesRoot, IEntityFactory
    {
        public EnginesRoot(NodeSubmissionScheduler nodeScheduler)
        {
            _nodeEngines = new Dictionary<Type, FasterList<INodeEngine>>();
            _engineRootWeakReference = new WeakReference(this);
            _otherEnginesReferences = new FasterList<IEngine>();

            _nodesDB = new Dictionary<Type, FasterList<INode>>();
            _nodesDBdic = new Dictionary<Type, Dictionary<int, INode>>();

            _nodesToAdd = new FasterList<INode>();
            _groupNodesToAdd = new FasterList<INode>();

            _nodesDBgroups = new Dictionary<Type, FasterList<INode>>();

            _scheduler = nodeScheduler;
            _scheduler.Schedule(SubmitNodes);

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

        public void AddEngine<T>(INodeEngine<T> engine) where T:class, INode
        {
            AddEngine(new NodeEngineWrapper<T>(engine));

            if (engine is IQueryableNodeEngine)
                (engine as IQueryableNodeEngine).nodesDB = new EngineNodeDB(_nodesDB, _nodesDBdic, _nodesDBgroups);
        }

        public void AddEngine<T, U>(INodeEngine<T, U> engine) where T:class, INode  where U:class, INode
        {
            AddEngine(new NodeEngineWrapper<T, U>(engine));
            AddEngine((INodeEngine<U>)(engine));
        }

        public void AddEngine<T, U, V>(INodeEngine<T, U, V> engine) where T:class, INode  
                                                                    where U:class, INode
                                                                    where V:class, INode
        {
            AddEngine(new NodeEngineWrapper<T, U, V>(engine));
            AddEngine((INodeEngine<U, V>)(engine));
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
            }
            else
            {
                var engineType = engine.GetType();

#if !NETFX_CORE
                var baseType = engineType.BaseType;

                if (baseType.IsGenericType
#else
                var baseType = engineType.GetTypeInfo().BaseType;

                if (baseType.IsConstructedGenericType
#endif
                && baseType.GetGenericTypeDefinition() == typeof(SingleNodeEngine<>))
                {
                    AddEngine(engine as INodeEngine, baseType.GetGenericArguments(), _nodeEngines);
                }
                else
                {
                    bool found = false;

                    for (int i = 0, maxLength = engineType.GetInterfaces().Length; i < maxLength; i++)
                    {
                        var type = engineType.GetInterfaces()[i];
                        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(INodeEngine<>))
                        {
                            AddEngine(engine as INodeEngine, type.GetGenericArguments(), _nodeEngines);

                            found = true;
                        }
                    }

                    if (found == false)
                        _otherEnginesReferences.Add(engine);
                }
            }
            
            if (engine is ICallBackOnAddEngine)
                (engine as ICallBackOnAddEngine).Ready();
        }

        public void BuildEntity(int ID, EntityDescriptor ed)
        {
            var entityNodes = ed.BuildNodes(ID, (nodes) =>
            {
                if (_engineRootWeakReference.IsValid == true)
                    InternalRemove(nodes);
            });
            
            _nodesToAdd.AddRange(entityNodes);
        }

        /// <summary>
        /// An entity group is a meta entity. It's a way to create a set of entitites that
        /// are not easily queriable otherwise. For example you may group existing entities
        /// by size and type and then use the groupID to retrieve a single node that is shared
        /// among the single entities of the same type and size. This willwd prevent the scenario
        /// where the coder is forced to parse all the entities to find the ones of the same
        /// size and type. Since the entity group is managed through the shared node, the same
        /// shared node must be found on the single entities of the same type and size.
        /// The shared node is then used by engines that are meant to manage a group of entities
        /// through a single node. The same engine can manage several groups of entitites.
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="ed"></param>
        public void BuildEntityGroup(int groupID, EntityDescriptor ed)
        {
            var entityNodes = ed.BuildNodes(groupID, (nodes) =>
            {
                if (_engineRootWeakReference.IsValid == true)
                    InternalGroupRemove(nodes);
            });

            _groupNodesToAdd.AddRange(entityNodes);
        }

        static void AddEngine(INodeEngine engine, Type[] types, Dictionary<Type, FasterList<INodeEngine>> engines)
        {
            for (int i = 0; i < types.Length; i++)
            {
                FasterList<INodeEngine> list;

                var type = types[i];

                if (engines.TryGetValue(type, out list) == false)
                {
                    list = new FasterList<INodeEngine>();

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

        void AddNodeToTheDB(INode node, Type nodeType)
        {
            FasterList<INode> nodes;
            if (_nodesDB.TryGetValue(nodeType, out nodes) == false)
                nodes = _nodesDB[nodeType] = new FasterList<INode>();

            nodes.Add(node);

            AddNodeToNodesDictionary(node, nodeType);
        }

        void AddNodeToNodesDictionary(INode node, Type nodeType)
        {
            if (node is NodeWithID)
            {
                Dictionary<int, INode> nodesDic;
                if (_nodesDBdic.TryGetValue(nodeType, out nodesDic) == false)
                    nodesDic = _nodesDBdic[nodeType] = new Dictionary<int, INode>();

                nodesDic[(node as NodeWithID).ID] = node;
            }
        }

        void AddNodeToTheSuitableEngines(INode node, Type nodeType)
        {
            FasterList<INodeEngine> enginesForNode;

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

        void RemoveNodesFromDB(Dictionary<Type, FasterList<INode>> DB, FasterReadOnlyList<INode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                FasterList<INode> nodesInDB;

                var node = nodes[i];
                var nodeType = node.GetType();
                
                if (DB.TryGetValue(nodeType, out nodesInDB) == true)
                    nodesInDB.UnorderredRemove(node); //should I remove it from the dictionary if length is zero?

                if (node is NodeWithID)
                {
                    Dictionary<int, INode> nodesDic;

                    if (_nodesDBdic.TryGetValue(nodeType, out nodesDic))
                        nodesDic.Remove((node as NodeWithID).ID);
                }
            }
        }

        void RemoveNodesFromEngines(FasterReadOnlyList<INode> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                FasterList<INodeEngine> enginesForNode;
                var node = nodes[i];

                if (_nodeEngines.TryGetValue(node.GetType(), out enginesForNode))
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
        }
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
        static void AddNodeToEngine(INodeEngine engine, INode node)
        {
            engine.Add(node);
        }

        static void RemoveNodeFromEngine(INodeEngine engine, INode node)
        {
            engine.Remove(node);
        }
#endif

        void InternalRemove(FasterReadOnlyList<INode> nodes)
        {
            RemoveNodesFromEngines(nodes);
            RemoveNodesFromDB(_nodesDB, nodes);
        }

        void InternalGroupRemove(FasterReadOnlyList<INode> nodes)
        {
            RemoveNodesFromEngines(nodes);
            RemoveNodesFromDB(_nodesDBgroups, nodes);
        }

        Dictionary<Type, FasterList<INodeEngine>>     _nodeEngines;
        FasterList<IEngine>                                  _otherEnginesReferences;

        Dictionary<Type, FasterList<INode>>       _nodesDB;
        Dictionary<Type, FasterList<INode>>       _nodesDBgroups;

        Dictionary<Type, Dictionary<int, INode>>  _nodesDBdic;

        FasterList<INode>                         _nodesToAdd;
        FasterList<INode>                         _groupNodesToAdd;

        WeakReference                             _engineRootWeakReference;
        NodeSubmissionScheduler                   _scheduler;
    }
}

