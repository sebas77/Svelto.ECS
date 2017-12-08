using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Legacy;
using Svelto.ECS.NodeSchedulers;
using Svelto.ECS.Profiler;
using Svelto.Utilities;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS.Internal
{
    struct BuildNodeCallbackStruct
    {
        public Action<FasterList<INodeBuilder>, int> internalRemove;
        public Action<FasterList<INodeBuilder>, int> internalEnable;
        public Action<FasterList<INodeBuilder>, int> internalDisable;            
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

            _engineRootWeakReference = new DataStructures.WeakReference<EnginesRoot>(this);

            _nodesDB = new Dictionary<Type, ITypeSafeList>();
            _metaNodesDB = new Dictionary<Type, ITypeSafeList>();
            _groupNodesDB = new Dictionary<int, Dictionary<Type, ITypeSafeList>>();
            _nodesDBdic = new Dictionary<Type, ITypeSafeDictionary>();
            
            _sharedStructNodeLists = new SharedStructNodeLists();
            _sharedGroupedStructNodeLists = new SharedGroupedStructNodesLists();

            _nodesToAdd = new DoubleBufferedNodes<Dictionary<Type, ITypeSafeList>>();           
            _metaNodesToAdd = new DoubleBufferedNodes<Dictionary<Type, ITypeSafeList>>();
            _groupedNodesToAdd = new DoubleBufferedNodes<Dictionary<int, Dictionary<Type, ITypeSafeList>>>(); 
            
            _callBackStructForBuiltGroupedNodes = new BuildNodeCallbackStruct();

            _callBackStructForBuiltGroupedNodes.internalRemove = InternalRemove;
            _callBackStructForBuiltGroupedNodes.internalDisable = InternalDisable;
            _callBackStructForBuiltGroupedNodes.internalEnable = InternalEnable;            
            
            _callBackStructForBuiltNodes = new BuildNodeCallbackStruct();

            _callBackStructForBuiltNodes.internalRemove = InternalGroupedRemove;
            _callBackStructForBuiltNodes.internalDisable = InternalDisable;
            _callBackStructForBuiltNodes.internalEnable = InternalEnable;
            
            _callBackStructForBuiltMetaNodes = new BuildNodeCallbackStruct();

            _callBackStructForBuiltMetaNodes.internalRemove = InternalMetaRemove;
            _callBackStructForBuiltMetaNodes.internalDisable = InternalDisable;
            _callBackStructForBuiltMetaNodes.internalEnable = InternalEnable;

            _scheduler = nodeScheduler;
            _scheduler.Schedule(SubmitNodes);

            _structNodeEngineType = typeof(IStructNodeEngine<>);
            _groupedStructNodesEngineType = typeof(IGroupedStructNodesEngine<>);
            _activableNodeEngineType = typeof(IActivableNodeEngine<>);
            
            _implementedInterfaceTypes = new Dictionary<Type, Type[]>();

#if UNITY_EDITOR
            UnityEngine.GameObject debugEngineObject = new UnityEngine.GameObject("Engine Debugger");
            debugEngineObject.gameObject.AddComponent<EngineProfilerBehaviour>();
#endif
        }

        public void BuildEntity(int ID, EntityDescriptor ed)
        {
            ed.BuildNodes(ID, _nodesToAdd.other, ref _callBackStructForBuiltNodes);
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
            ed.BuildNodes(metaEntityID, _metaNodesToAdd.other, ref _callBackStructForBuiltMetaNodes);
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
            ed.BuildGroupedNodes(entityID, groupID, _groupedNodesToAdd.other, ref _callBackStructForBuiltGroupedNodes);
        }

        public void AddEngine(IEngine engine)
        {
#if UNITY_EDITOR
            EngineProfiler.AddEngine(engine);
#endif
            var queryableNodeEngine = engine as IQueryableNodeEngine;
                    if (queryableNodeEngine != null)
                          queryableNodeEngine.nodesDB =
                                new EngineNodeDB(_nodesDB, _nodesDBdic, _metaNodesDB, _groupNodesDB);

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

            if (_implementedInterfaceTypes.ContainsKey(_structNodeEngineType))
            {
                ((IStructNodeEngine)engine).CreateStructNodes
                    (_sharedStructNodeLists);
            }

            if (_implementedInterfaceTypes.ContainsKey(_groupedStructNodesEngineType))
            {
                ((IGroupedStructNodesEngine)engine).CreateStructNodes
                    (_sharedGroupedStructNodeLists);
            }

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
                AddNodeToDB(nodesDB, nodeList);

                if (nodeList.Value.isQueryiableNode)
                {
                    AddNodeToNodesDictionary(nodesDBdic, nodeList.Value, nodeList.Key);
                    
                    foreach (var node in nodeList.Value)
                        AddNodeToTheSuitableEngines(nodeEngines, node as NodeWithID, nodeList.Key);
                }
            }
        }

        static void AddGroupNodesToTheDBAndSuitableEngines(Dictionary<int, Dictionary<Type, ITypeSafeList>> groupedNodesToAdd,
                                                      Dictionary<Type, FasterList<IEngine>> nodeEngines, 
                                                      Dictionary<Type, ITypeSafeDictionary> nodesDBdic, 
                                                      Dictionary<int, Dictionary<Type, ITypeSafeList>> groupNodesDB,
                                                      Dictionary<Type, ITypeSafeList> nodesDB)
        {
            foreach (var group in groupedNodesToAdd)
            {
                AddNodesToTheDBAndSuitableEngines(group.Value, nodeEngines, nodesDBdic, nodesDB);

                AddNodesToGroupDB(groupNodesDB, @group);
            }
        }

        static void AddNodesToGroupDB(Dictionary<int, Dictionary<Type, ITypeSafeList>> groupNodesDB, 
                                      KeyValuePair<int, Dictionary<Type, ITypeSafeList>> @group)
        {
            Dictionary<Type, ITypeSafeList> groupedNodesByType;

            if (groupNodesDB.TryGetValue(@group.Key, out groupedNodesByType) == false)
                groupedNodesByType = groupNodesDB[@group.Key] = new Dictionary<Type, ITypeSafeList>();

            foreach (var node in @group.Value)
            {
                groupedNodesByType.Add(node.Key, node.Value);
            }
        }

        static void AddNodeToDB(Dictionary<Type, ITypeSafeList> nodesDB, KeyValuePair<Type, ITypeSafeList> nodeList)
        {
            ITypeSafeList dbList;

            if (nodesDB.TryGetValue(nodeList.Key, out dbList) == false)
                dbList = nodesDB[nodeList.Key] = nodeList.Value.Create();

            dbList.AddRange(nodeList.Value);
        }
        
        static void AddNodeToNodesDictionary(Dictionary<Type, ITypeSafeDictionary> nodesDBdic, 
                                             ITypeSafeList nodes, Type nodeType) 
        {
            ITypeSafeDictionary nodesDic;
            
            if (nodesDBdic.TryGetValue(nodeType, out nodesDic) == false)
                nodesDic = nodesDBdic[nodeType] = nodes.CreateIndexedDictionary();

            nodesDic.FillWithIndexedNodes(nodes);
        }

        static void AddNodeToTheSuitableEngines(Dictionary<Type, FasterList<IEngine>> nodeEngines, NodeWithID node, Type nodeType)
        {
            FasterList<IEngine> enginesForNode;

            if (nodeEngines.TryGetValue(nodeType, out enginesForNode))
            {
                for (int j = 0; j < enginesForNode.Count; j++)
                {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
                    EngineProfiler.MonitorRemoveDuration(RemoveNodeFromEngine, (enginesForNode[j] as INodeEngine), node);
#else
                    (enginesForNode[j] as INodeEngine).Add(node);
#endif
                }
            }
        }
/*
        void DisableNodeFromEngines(INode node, Type nodeType)
        {
            ITypeSafeList enginesForNode;

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
            ITypeSafeList enginesForNode;

            if (_activableEngines.TryGetValue(nodeType, out enginesForNode))
            {
                for (int j = 0; j < enginesForNode.Count; j++)
                {
                    (enginesForNode[j] as IActivableNodeEngine).Enable(node);
                }
            }
        }*/
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

        void InternalDisable(FasterList<INodeBuilder> nodeBuilders, int entityID)
        {
/*            if (_engineRootWeakReference.IsValid == false)
                return;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                Type nodeType = node.GetType();
                
                DisableNodeFromEngines(node, nodeType);
            }*/
        }

        void InternalEnable(FasterList<INodeBuilder> nodeBuilders, int entityID)
        {/*
            if (_engineRootWeakReference.IsValid == false)
                return;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                Type nodeType = node.GetType();
                EnableNodeFromEngines(node, nodeType);
            }*/
        }

        void InternalRemove(FasterList<INodeBuilder> nodeBuilders, int entityID)
        {
            if (_engineRootWeakReference.IsValid == false)
                return;

            int nodeBuildersCount = nodeBuilders.Count;
            for (int i = 0; i < nodeBuildersCount; i++)
            {
                Type nodeType = nodeBuilders[i].GetType();

                ITypeSafeList nodes;
                if (_nodesDB.TryGetValue(nodeType, out nodes) == true)
                    nodes.UnorderedRemove(entityID);

                if (nodes.isQueryiableNode)
                {
                    var node = _nodesDBdic[nodeType].GetIndexedNode(entityID);
                    
                    _nodesDBdic[nodeType].Remove(entityID);

                    RemoveNodeFromEngines(_nodeEngines, node, nodeType);
                }
            }
        }
        
        void InternalGroupedRemove(FasterList<INodeBuilder> nodeBuilders, int entityID)
        {
        }

        void InternalMetaRemove(FasterList<INodeBuilder> nodeBuilders, int entityID)
        {
        }
        
        static void RemoveNodeFromEngines(Dictionary<Type, FasterList<IEngine>> nodeEngines, NodeWithID node, Type nodeType)
        {
            FasterList<IEngine> enginesForNode;

            if (nodeEngines.TryGetValue(nodeType, out enginesForNode))
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

        void SubmitNodes()
        {
            _nodesToAdd.Swap();
            _metaNodesToAdd.Swap();
            _groupedNodesToAdd.Swap();
            
            bool newNodesHaveBeenAddedWhileIterating =
                _metaNodesToAdd.Count > 0 
                || _nodesToAdd.Count > 0
                || _groupedNodesToAdd.Count > 0;

            int numberOfReenteringLoops = 0;

            while (newNodesHaveBeenAddedWhileIterating)
            {
                if ( _nodesToAdd.Count > 0)
                    AddNodesToTheDBAndSuitableEngines(_nodesToAdd.current, _nodeEngines, _nodesDBdic, _nodesDB);
                
                if ( _metaNodesToAdd.Count > 0)
                    AddNodesToTheDBAndSuitableEngines(_metaNodesToAdd.current, _nodeEngines, _nodesDBdic, _metaNodesDB);
                
                if (_groupedNodesToAdd.Count > 0)
                    AddGroupNodesToTheDBAndSuitableEngines(_groupedNodesToAdd.current, _nodeEngines, _nodesDBdic, _groupNodesDB, _nodesDB);
                
                _nodesToAdd.Clear();
                _metaNodesToAdd.Clear();
                _groupedNodesToAdd.Clear();

                _nodesToAdd.Swap();
                _metaNodesToAdd.Swap();
                _groupedNodesToAdd.Swap();
                
                newNodesHaveBeenAddedWhileIterating =
                    _metaNodesToAdd.Count > 0 
                    || _nodesToAdd.Count > 0
                    || _groupedNodesToAdd.Count > 0;

                if (numberOfReenteringLoops > 5)
                    throw new Exception("possible infinite loop found creating Entities inside INodesEngine Add method, please consider building entities outside INodesEngine Add method");

                numberOfReenteringLoops++;
            } 
        }

        readonly Dictionary<Type, FasterList<IEngine>> _nodeEngines;
        readonly Dictionary<Type, FasterList<IEngine>> _activableEngines;

        readonly FasterList<IEngine> _otherEngines;

        readonly Dictionary<Type, ITypeSafeList> _nodesDB;
        readonly Dictionary<Type, ITypeSafeList> _metaNodesDB;
        readonly Dictionary<int, Dictionary<Type, ITypeSafeList>> _groupNodesDB;
        
        readonly Dictionary<Type, ITypeSafeDictionary> _nodesDBdic;

        readonly DoubleBufferedNodes<Dictionary<Type, ITypeSafeList>> _nodesToAdd;
        readonly DoubleBufferedNodes<Dictionary<Type, ITypeSafeList>> _metaNodesToAdd;
        readonly DoubleBufferedNodes<Dictionary<int, Dictionary<Type, ITypeSafeList>>> _groupedNodesToAdd;
      
        readonly NodeSubmissionScheduler _scheduler;

        readonly Type _structNodeEngineType;
        readonly Type _groupedStructNodesEngineType;
        readonly Type _activableNodeEngineType;
        
        readonly SharedStructNodeLists _sharedStructNodeLists;
        readonly SharedGroupedStructNodesLists _sharedGroupedStructNodeLists;

        readonly Dictionary<Type, Type[]>                     _implementedInterfaceTypes;
        readonly DataStructures.WeakReference<EnginesRoot>    _engineRootWeakReference;
        
        BuildNodeCallbackStruct _callBackStructForBuiltNodes;
        BuildNodeCallbackStruct _callBackStructForBuiltGroupedNodes;
        BuildNodeCallbackStruct _callBackStructForBuiltMetaNodes;

        class DoubleBufferedNodes<T> where T : class, IDictionary, new()
        {
            readonly T _nodesToAddBufferA = new T();
            readonly T _nodesToAddBufferB = new T();

            public DoubleBufferedNodes()
            {
                this.other = _nodesToAddBufferA;
                this.current = _nodesToAddBufferB;
            }

            public T other  { get; private set; }
            public T current { get; private set; }

            public int Count
            {
                get { return current.Count; }
            }
            
            public void Clear()
            {
                current.Clear();
            }

            public void Swap()
            {
                var toSwap = other;
                other = current;
                current = toSwap;
            }
        }
    }
}