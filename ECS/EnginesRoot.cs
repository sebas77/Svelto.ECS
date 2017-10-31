using System;
using System.Collections.Generic;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.NodeSchedulers;
using WeakReference = Svelto.DataStructures.WeakReference<Svelto.ECS.EnginesRoot>;

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
            _nodeEngines = new Dictionary<Type, FasterList<IEngine>>();
            _activableEngines = new Dictionary<Type, FasterList<IEngine>>();
            _otherEngines = new FasterList<IEngine>();

            _engineRootWeakReference = new WeakReference(this);

            _nodesDB = new Dictionary<Type, FasterList<INode>>();
            _nodesDBdic = new Dictionary<Type, Dictionary<int, INode>>();

            _nodesToAdd = new FasterList<INode>();
            _metaNodesToAdd = new FasterList<INode>();

            _metaNodesDB = new Dictionary<Type, FasterList<INode>>();
            _sharedStructNodeLists = new SharedStructNodeLists();
            _sharedGroupedStructNodeLists = new SharedGroupedStructNodesLists();

            _internalRemove = InternalRemove;
            _internalDisable = InternalDisable;
            _internalEnable = InternalEnable;
            _internalMetaRemove = InternalMetaRemove;

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
                var nodesToAdd = _nodesToAdd.ToArrayFast();

                for (int i = startNodes; i < nodesCount; i++)
                {
                    var node = nodesToAdd[i];
                    var nodeType = node.GetType();

                    AddNodeToTheDB(node, nodeType);
                    var nodeWithId = node as INodeWithID;
                    if (nodeWithId != null)
                        AddNodeToNodesDictionary(nodeWithId, nodeType);
                }

                var metaNodesToAdd = _metaNodesToAdd.ToArrayFast();

                for (int i = startMetaNodes; i < metaNodesCount; i++)
                {
                    var node = metaNodesToAdd[i];
                    var nodeType = node.GetType();

                    AddNodeToMetaDB(node, nodeType);
                    var nodeWithId = node as INodeWithID;
                    if (nodeWithId != null)
                        AddNodeToNodesDictionary(nodeWithId, nodeType);
                }

                for (int i = startNodes; i < nodesCount; i++)
                {
                    var node = nodesToAdd[i];
                    AddNodeToTheSuitableEngines(node, node.GetType());
                }

                for (int i = startMetaNodes; i < metaNodesCount; i++)
                {
                    var node = metaNodesToAdd[i];
                    AddNodeToTheSuitableEngines(node, node.GetType());
                }

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
#if !NETFX_CORE

                if (false == interfaceType.IsGenericType)
#else
                if (false == interfaceType.IsConstructedGenericType)
#endif
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
#if !NETFX_CORE
            var baseType = engineType.BaseType;
            
            if (baseType.IsGenericType
#else
                var baseType = engineType.GetTypeInfo().BaseType;

                if (baseType.IsConstructedGenericType
#endif
                && engine is INodeEngine)
            {
                AddEngine(engine as INodeEngine, baseType.GetGenericArguments(), _nodeEngines);

                engineAdded = true;

                return true;
            }

            return false;
        }

        public void BuildEntity(int ID, EntityDescriptor ed)
        {
            var entityNodes = ed.BuildNodes(ID,
                _internalRemove,
                _internalEnable,
                _internalDisable
            );

            _nodesToAdd.AddRange(entityNodes);
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
        /// </summary>
        /// <param name="metaEntityID"></param>
        /// <param name="ed"></param>
        public void BuildMetaEntity(int metaEntityID, EntityDescriptor ed)
        {
            var entityNodes = ed.BuildNodes(metaEntityID,
                _internalMetaRemove,
                _internalEnable,
                _internalDisable
                );

            _metaNodesToAdd.AddRange(entityNodes);
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
            var entityNodes = ed.BuildNodes(entityID,
                _internalRemove,
                _internalEnable,
                _internalDisable
                );

            _nodesToAdd.AddRange(entityNodes);

            for (int i = 0; i < entityNodes.Count; i++)
            {
                var groupNode = entityNodes[i] as IGroupedStructNodeWithID;
                if (groupNode != null)
                    groupNode.groupID = groupID;
            }
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

        void AddNodeToMetaDB(INode node, Type nodeType)
        {
            FasterList<INode> nodes;
            if (_metaNodesDB.TryGetValue(nodeType, out nodes) == false)
                nodes = _metaNodesDB[nodeType] = new FasterList<INode>();

            nodes.Add(node);
        }

        void AddNodeToTheDB<T>(T node, Type nodeType) where T : INode
        {
            FasterList<INode> nodes;
            if (_nodesDB.TryGetValue(nodeType, out nodes) == false)
                nodes = _nodesDB[nodeType] = new FasterList<INode>();

            nodes.Add(node);
        }

        void AddNodeToNodesDictionary<T>(T node, Type nodeType) where T : INodeWithID
        {
            Dictionary<int, INode> nodesDic;

            if (_nodesDBdic.TryGetValue(nodeType, out nodesDic) == false)
                nodesDic = _nodesDBdic[nodeType] = new Dictionary<int, INode>();

            nodesDic.Add(node.ID, node);
        }

        void AddNodeToTheSuitableEngines(INode node, Type nodeType)
        {
            FasterList<IEngine> enginesForNode;

            if (_nodeEngines.TryGetValue(nodeType, out enginesForNode))
            {
                for (int j = 0; j < enginesForNode.Count; j++)
                {
#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
                    EngineProfiler.MonitorAddDuration(AddNodeToEngine, enginesForNode[j] as INodeEngine, node);
#else
                    (enginesForNode[j] as INodeEngine).Add(node);
#endif
                }
            }
        }

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

        void InternalRemove(FasterList<INode> nodes)
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

        readonly Dictionary<Type, FasterList<IEngine>> _nodeEngines;
        readonly Dictionary<Type, FasterList<IEngine>> _activableEngines;

        readonly FasterList<IEngine> _otherEngines;

        readonly Dictionary<Type, FasterList<INode>> _nodesDB;
        readonly Dictionary<Type, FasterList<INode>> _metaNodesDB;

        readonly Dictionary<Type, Dictionary<int, INode>> _nodesDBdic;

        readonly FasterList<INode> _nodesToAdd;
        readonly FasterList<INode> _metaNodesToAdd;

        readonly WeakReference _engineRootWeakReference;
        readonly SharedStructNodeLists _sharedStructNodeLists;
        readonly SharedGroupedStructNodesLists _sharedGroupedStructNodeLists;

        readonly NodeSubmissionScheduler _scheduler;

        readonly Action<FasterList<INode>> _internalRemove;
        readonly Action<FasterList<INode>> _internalEnable;
        readonly Action<FasterList<INode>> _internalDisable;
        readonly Action<FasterList<INode>> _internalMetaRemove;

        readonly Type _structNodeEngineType;
        readonly Type _groupedStructNodesEngineType;
        readonly Type _activableNodeEngineType;

        readonly Dictionary<Type, Type[]> _implementedInterfaceTypes;
    }
}

