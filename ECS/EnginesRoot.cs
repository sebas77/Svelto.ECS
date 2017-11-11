using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.NodeSchedulers;
using System.Reflection;
using Svelto.Utilities;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Persistence;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS.Internal
{
    struct BuildNodeCallbackStruct
    {
        public Action<ITypeSafeList> _internalRemove;
        public Action<ITypeSafeList> _internalEnable;
        public Action<ITypeSafeList> _internalDisable;            
    }
}

namespace Svelto.ECS
{
    public sealed class EnginesRoot : IEnginesRoot, IEntityFactory
    {
        public EnginesRoot(NodeSubmissionScheduler nodeScheduler)
        {
            _nodeEngines = new Dictionary<Type, FasterList<IEngine>>();
            _activableEngines = new Dictionary<Type, FasterList<IEngine>>();
            _otherEngines = new FasterList<IEngine>();

            //_engineRootWeakReference = new DataStructures.WeakReference<EnginesRoot>(this);

            _nodesDB = new Dictionary<Type, ITypeSafeList>();
            _metaNodesDB = new Dictionary<Type, ITypeSafeList>();
            _nodesDBdic = new Dictionary<Type, ITypeSafeDictionary>();

            _nodesToAdd = new Dictionary<Type, ITypeSafeList>();
            _metaNodesToAdd = new Dictionary<Type, ITypeSafeList>();
            _groupedNodesToAdd = new Dictionary<Type, Dictionary<int, ITypeSafeList>>();
            
            _callBackStruct = new BuildNodeCallbackStruct();

       /*     _callBackStruct._internalRemove = InternalRemove;
            _callBackStruct._internalDisable = InternalDisable;
            _callBackStruct._internalEnable = InternalEnable;
            _callBackStruct._internalMetaRemove = InternalMetaRemove;*/

            _scheduler = nodeScheduler;
            _scheduler.Schedule(SubmitNodes);

            _structNodeEngineType = typeof(IStructNodeEngine<>);
            _groupedStructNodesEngineType = typeof(IGroupedStructNodesEngine<>);
            _activableNodeEngineType = typeof(IActivableNodeEngine<>);
            
            _implementedInterfaceTypes = new Dictionary<Type, Type[]>();

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
            GameObject debugEngineObject = new GameObject("Engine Debugger");
            debugEngineObject.gameObject.AddComponent<EngineProfilerBehaviour>();
#endif
        }

        public void BuildEntity(int ID, EntityDescriptor ed)
        {
            ed.BuildNodes(ID, _nodesToAdd, ref _callBackStruct);
        }

        /// <summary>
        /// A meta entity is a way to manage a set of entitites that are not easily 
        /// queriable otherwise. For example you may want to group existing entities
        /// by size and type and then use the meta entity node to manage the data 
        /// shared among the single entities of the same type and size. This will 
        /// prevent the scenario where the coder is forced to parse all the entities to 
        /// find the ones of the same size and type. 
        /// Since the entities are managed through the shared node, the same
        /// shared node must be found on the single entities of the same type and size.
        /// The shared node of the meta entity is then used by engines that are meant 
        /// to manage a group of entities through a single node. 
        /// The same engine can manage several meta entities nodes too.
        /// The Engine manages the logic of the Meta Node data and other engines
        /// can read back this data through the normal entity as the shared node
        /// will be present in their descriptor too.
        /// It's a way to control a group of Entities through a node only.
        /// This set of entities can share exactly the same node reference if 
        /// built through this function. In this way, if you need to set a variable
        /// on a group of entities, instead to inject N nodes and iterate over
        /// them to set the same value, you can inject just one node, set the value
        /// and be sure that the value is shared between entities.
        /// </summary>
        /// <param name="metaEntityID"></param>
        /// <param name="ed"></param>
        public void BuildMetaEntity(int metaEntityID, EntityDescriptor ed)
        {
            ed.BuildNodes(metaEntityID, _metaNodesToAdd, ref _callBackStruct);
        }

        /// <summary>
        /// Using this function is like building a normal entity, but the nodes
        /// are grouped by groupID to be better processed inside engines and
        /// improve cache locality. Only IGroupStructNodeWithID nodes are grouped
        /// other nodes are managed as usual.
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="groupID"></param>
        /// <param name="ed"></param>
        public void BuildEntityInGroup(int entityID, int groupID,
                    EntityDescriptor ed)
        {
            ed.BuildGroupedNodes(entityID, groupID, _groupedNodesToAdd, ref _callBackStruct);
        }

        public void AddEngine(IEngine engine)
        {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
            EngineProfiler.AddEngine(engine);
#endif
            var queryableNodeEngine = engine as IQueryableNodeEngine;
                    if (queryableNodeEngine != null)
                          queryableNodeEngine.nodesDB =
                                new EngineNodeDB(_nodesDB, _nodesDBdic, _metaNodesDB);

            var engineType = engine.GetType();
            var implementedInterfaces = engineType.GetInterfaces();

            CollectImplementedInterfaces(implementedInterfaces);

            var engineAdded = CheckGenericEngines(engine);

            if (CheckLegacyNodesEngine(engine, ref engineAdded) == false)
                CheckNodesEngine(engine, engineType, ref engineAdded);

            if (engineAdded == false)
                _otherEngines.Add(engine);

            var callBackOnAddEngine = engine as ICallBackOnAddEngine;
            if (callBackOnAddEngine != null)
                callBackOnAddEngine.Ready();
        }

        void CollectImplementedInterfaces(Type[] implementedInterfaces)
        {
            _implementedInterfaceTypes.Clear();

            var type = typeof(IEngine);

            for (int index = 0; index < implementedInterfaces.Length; index++)
            {
                var interfaceType = implementedInterfaces[index];

                if (type.IsAssignableFrom(interfaceType) == false)
                    continue;

                if (false == interfaceType.IsGenericTypeEx())
                {
                    continue;
                }

                var genericTypeDefinition = interfaceType.GetGenericTypeDefinition();

                _implementedInterfaceTypes.Add(genericTypeDefinition, interfaceType.GetGenericArguments());
            }
        }

        bool CheckGenericEngines(IEngine engine)
        {
            if (_implementedInterfaceTypes.Count == 0) return false;

            bool engineAdded = false;

            Type[] arguments;
            if (_implementedInterfaceTypes.TryGetValue(_activableNodeEngineType, 
                                                       out arguments))
            {
                AddEngine(engine, arguments, _activableEngines);

                engineAdded = true;
            }

            return engineAdded;
        }

        bool CheckLegacyNodesEngine(IEngine engine, ref bool engineAdded)
        {
            var nodesEngine = engine as INodesEngine;
            if (nodesEngine != null)
            {
                AddEngine(nodesEngine, nodesEngine.AcceptedNodes(), _nodeEngines);

                engineAdded = true;

                return true;
            }

            return false;
        }

        bool CheckNodesEngine(IEngine engine, Type engineType, ref bool engineAdded)
        {
            var baseType = engineType.GetBaseType();
            
            if (baseType.IsGenericTypeEx()
                && engine is INodeEngine)
            {
                AddEngine(engine as INodeEngine, baseType.GetGenericArguments(), _nodeEngines);

                engineAdded = true;

                return true;
            }

            return false;
        }

        static void AddEngine(IEngine engine, Type[] types,
                              Dictionary<Type, FasterList<IEngine>> engines)
        {
            for (int i = 0; i < types.Length; i++)
            {
                FasterList<IEngine> list;

                var type = types[i];

                if (engines.TryGetValue(type, out list) == false)
                {
                    list = new FasterList<IEngine>();

                    engines.Add(type, list);
                }

                list.Add(engine);
            }
        }

        static void AddNodesToTheDBAndSuitableEngines(Dictionary<Type, ITypeSafeList> nodesToAdd, 
            Dictionary<Type, FasterList<IEngine>> nodeEngines, 
            Dictionary<Type, ITypeSafeDictionary> nodesDBdic, 
            Dictionary<Type, ITypeSafeList> nodesDB)
        {
            foreach (var nodeList in nodesToAdd)
            {
                ITypeSafeList dbList;

                if (nodesDB.TryGetValue(nodeList.Key, out dbList) == false)
                    dbList = nodesDB[nodeList.Key] = nodeList.Value.Create();

                dbList.AddRange(nodeList.Value);

                AddNodeToNodesDictionary(nodesDBdic, nodeList.Value, nodeList.Key);
                AddNodesToTheSuitableEngines(nodeEngines, nodeList.Value, nodeList.Key);
            }
        }
        
        static void AddNodeToNodesDictionary(Dictionary<Type, ITypeSafeDictionary> nodesDBdic, ITypeSafeList nodes, Type nodeType) 
        {
            ITypeSafeDictionary nodesDic;

            if (nodesDBdic.TryGetValue(nodeType, out nodesDic) == false)
                nodesDic = nodesDBdic[nodeType] = nodes.CreateIndexedDictionary();

            nodes.AddToIndexedDictionary(nodesDic);
        }

        static void AddNodesToTheSuitableEngines(Dictionary<Type, FasterList<IEngine>> nodeEngines, ITypeSafeList nodes, Type nodeType)
        {
            FasterList<IEngine> enginesForNode;

            if (nodeEngines.TryGetValue(nodeType, out enginesForNode))
            {
                for (int j = 0; j < enginesForNode.Count; j++)
                {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
                    EngineProfiler.MonitorAddDuration(AddNodeToEngine, enginesForNode[j] as INodeEngine, node);
#else
                    (enginesForNode[j] as INodeEngine).Add(nodes);
#endif
                }
            }
        }
        /*
                void RemoveNodeFromTheDB<T>(T node, Type nodeType) where T : INode
                {
                    FasterList<INode> nodes;
                    if (_nodesDB.TryGetValue(nodeType, out nodes) == true)
                        nodes.UnorderedRemove(node); //should I remove it from the dictionary if length is zero?
                }

                void RemoveNodeFromMetaDB<T>(T node, Type nodeType) where T : INode
                {
                    FasterList<INode> nodes;
                    if (_metaNodesDB.TryGetValue(nodeType, out nodes) == true)
                        nodes.UnorderedRemove(node); //should I remove it from the dictionary if length is zero?
                }

                void RemoveNodeFromNodesDictionary<T>(T node, Type nodeType) where T : INodeWithID
                {
                    Dictionary<int, INode> nodesDic;

                    if (_nodesDBdic.TryGetValue(nodeType, out nodesDic))
                        nodesDic.Remove(node.ID);
                }

                void RemoveNodeFromEngines<T>(T node, Type nodeType) where T : INode
                {
                    FasterList<IEngine> enginesForNode;

                    if (_nodeEngines.TryGetValue(nodeType, out enginesForNode))
                    {
                        for (int j = 0; j < enginesForNode.Count; j++)
                        {
        #if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
                            EngineProfiler.MonitorRemoveDuration(RemoveNodeFromEngine, (enginesForNode[j] as INodeEngine), node);
        #else
                            (enginesForNode[j] as INodeEngine).Remove(node);
        #endif
                        }
                    }
                }

                void DisableNodeFromEngines(INode node, Type nodeType)
                {
                    FasterList<IEngine> enginesForNode;

                    if (_activableEngines.TryGetValue(nodeType, out enginesForNode))
                    {
                        for (int j = 0; j < enginesForNode.Count; j++)
                        {
                            (enginesForNode[j] as IActivableNodeEngine).Disable(node);
                        }
                    }
                }

                void EnableNodeFromEngines(INode node, Type nodeType)
                {
                    FasterList<IEngine> enginesForNode;

                    if (_activableEngines.TryGetValue(nodeType, out enginesForNode))
                    {
                        for (int j = 0; j < enginesForNode.Count; j++)
                        {
                            (enginesForNode[j] as IActivableNodeEngine).Enable(node);
                        }
                    }
                }
        #if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
                void AddNodeToEngine(IEngine engine, INode node)
                {
                    (engine as INodeEngine).Add(node);
                }

                void RemoveNodeFromEngine(IEngine engine, INode node)
                {
                    (engine as INodeEngine).Remove(node);
                }
        #endif
        /*
                void InternalDisable(FasterList<INode> nodes)
                {
                    if (_engineRootWeakReference.IsValid == false)
                        return;

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        Type nodeType = node.GetType();
                        DisableNodeFromEngines(node, nodeType);
                    }
                }

                void InternalEnable(FasterList<INode> nodes)
                {
                    if (_engineRootWeakReference.IsValid == false)
                        return;

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        Type nodeType = node.GetType();
                        EnableNodeFromEngines(node, nodeType);
                    }
                }

                void InternalRemove(IFasterList nodes)
                {
                    if (_engineRootWeakReference.IsValid == false)
                        return;

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        Type nodeType = node.GetType();

                        RemoveNodeFromEngines(node, nodeType);
                        RemoveNodeFromTheDB(node, node.GetType());

                        var nodeWithId = node as INodeWithID;
                        if (nodeWithId != null)
                            RemoveNodeFromNodesDictionary(nodeWithId, nodeType);
                    }
                }

                void InternalMetaRemove(FasterList<INode> nodes)
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        var node = nodes[i];
                        Type nodeType = node.GetType();

                        RemoveNodeFromEngines(node, nodeType);
                        RemoveNodeFromMetaDB(node, nodeType);

                        var nodeWithId = node as INodeWithID;
                        if (nodeWithId != null)
                            RemoveNodeFromNodesDictionary(nodeWithId, nodeType);
                    }
                }

                readonly DataStructures.WeakReference<EnginesRoot>    _engineRootWeakReference;

                */

        void SubmitNodes()
        {
            int metaNodesCount = _metaNodesToAdd.Count;
            int nodesCount = _nodesToAdd.Count;

            if (metaNodesCount + nodesCount == 0) return;

            bool newNodesHaveBeenAddedWhileIterating;
            int startNodes = 0;
            int startMetaNodes = 0;
            int numberOfReenteringLoops = 0;

            do
            {
                AddNodesToTheDBAndSuitableEngines(_nodesToAdd, _nodeEngines, _nodesDBdic, _nodesDB);
                AddNodesToTheDBAndSuitableEngines(_metaNodesToAdd, _nodeEngines, _nodesDBdic, _metaNodesDB);

                newNodesHaveBeenAddedWhileIterating =
                    _metaNodesToAdd.Count > metaNodesCount ||
                    _nodesToAdd.Count > nodesCount;

                startNodes = nodesCount;
                startMetaNodes = metaNodesCount;

                if (numberOfReenteringLoops > 5)
                    throw new Exception("possible infinite loop found creating Entities inside INodesEngine Add method, please consider building entities outside INodesEngine Add method");

                numberOfReenteringLoops++;

                metaNodesCount = _metaNodesToAdd.Count;
                nodesCount = _nodesToAdd.Count;

            } while (newNodesHaveBeenAddedWhileIterating);

            _nodesToAdd.Clear();
            _metaNodesToAdd.Clear();
        }

        readonly Dictionary<Type, FasterList<IEngine>> _nodeEngines;
        readonly Dictionary<Type, FasterList<IEngine>> _activableEngines;

        readonly FasterList<IEngine> _otherEngines;

        readonly Dictionary<Type, ITypeSafeList> _nodesDB;
        readonly Dictionary<Type, ITypeSafeList> _metaNodesDB;
        readonly Dictionary<Type, Dictionary<int, ITypeSafeList>> _groupNodesDB;
        
        readonly Dictionary<Type, ITypeSafeDictionary> _nodesDBdic;

        /// <summary>
        /// Need to think about how to make BuildEntity thread safe as well
        /// </summary>
        readonly Dictionary<Type, ITypeSafeList> _nodesToAdd;
        readonly Dictionary<Type, ITypeSafeList> _metaNodesToAdd;
        readonly Dictionary<Type, Dictionary<int, ITypeSafeList>> _groupedNodesToAdd;
      
        readonly NodeSubmissionScheduler _scheduler;

        readonly Type _structNodeEngineType;
        readonly Type _groupedStructNodesEngineType;
        readonly Type _activableNodeEngineType;

        readonly Dictionary<Type, Type[]> _implementedInterfaceTypes;
        
        BuildNodeCallbackStruct _callBackStruct;
    }
}