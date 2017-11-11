using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public class EntityDescriptor
    {
        protected EntityDescriptor()
        {}

        /// <summary>
        /// if you want to avoid allocation in run-time, you can prebuild
        /// EntityDescriptors and use them to build entities at different
        /// times 
        /// </summary>        
        protected EntityDescriptor(INodeBuilder[] nodesToBuild)
        {
            _nodesToBuild = new FasterList<INodeBuilder>(nodesToBuild);
        }
        protected EntityDescriptor(INodeBuilder[] nodesToBuild, 
                                   params object[] componentsImplementor):this(nodesToBuild)
        {
            ProcessImplementors(componentsImplementor);
        }

        public void AddImplementors(params object[] componentsImplementor)
        {
            ProcessImplementors(componentsImplementor);
        }
        
        public void AddNodes(params INodeBuilder[] nodesWithID)
        {
            _nodesToBuild.AddRange(nodesWithID);
        }

        internal void BuildGroupedNodes
            (int entityID, int groupID, 
             Dictionary<Type, Dictionary<int, ITypeSafeList>> groupNodes, 
             ref BuildNodeCallbackStruct callBackstruct)
        {
            for (int index = 0; index < _nodesToBuild.Count; index++)
            {
                var nodeBuilder = _nodesToBuild[index];
                var nodeType = nodeBuilder.GetNodeType();
                
                Dictionary<int, ITypeSafeList> groupedNodesTyped;
                  
                if (groupNodes.TryGetValue(nodeType, out groupedNodesTyped) == false)
                {
                    groupedNodesTyped = new Dictionary<int, ITypeSafeList>(); 
                      
                    groupNodes.Add(nodeType, groupedNodesTyped);
                };
                  
                ITypeSafeList nodes;

                var mustAdd = groupedNodesTyped.TryGetValue(groupID, out nodes) == false;

                var node = nodeBuilder.BuildAndAddToList(ref nodes, entityID);

                if (mustAdd)
                    groupedNodesTyped[groupID] = nodes;

                if (node != null && nodeBuilder.reflects != FillNodeMode.None)
                {
                    node = FillNode(node, nodeBuilder.reflects);
                    
                    SetupImplementors(ref callBackstruct, nodes);
                }

            /*    var groupNode = node as IGroupedNode;
                if (groupNode != null)
                    groupNode.groupID = groupID;*/
            }
        }

        internal void BuildNodes(int entityID, 
             Dictionary<Type, ITypeSafeList> nodesToAdd, 
             ref BuildNodeCallbackStruct callBackstruct)
        {
            for (int index = 0; index < _nodesToBuild.Count; index++)
            {
                var nodeBuilder = _nodesToBuild[index];
                var nodeType = nodeBuilder.GetNodeType();

                ITypeSafeList nodes;

                var mustAdd = nodesToAdd.TryGetValue(nodeType, out nodes) == false;
                 
                var node = nodeBuilder.BuildAndAddToList(ref nodes, entityID);

                if (mustAdd)
                    nodesToAdd[nodeType] = nodes;

                if (node != null && nodeBuilder.reflects != FillNodeMode.None)
                {
                    FillNode(node, nodeBuilder.reflects);
                    
                    SetupImplementors(ref callBackstruct, nodes);
                }
            }
        }
        
        void ProcessImplementors(object[] implementors)
        {
            for (int index = 0; index < implementors.Length; index++)
            {
                var implementor = implementors[index];
                
                if (implementor != null)
                {
                    if (implementor is IRemoveEntityComponent)
                        _removingImplementors.Add(new DataStructures.WeakReference<IRemoveEntityComponent>(implementor as IRemoveEntityComponent));
                    if (implementor is IDisableEntityComponent)
                        _disablingImplementors.Add(new DataStructures.WeakReference<IDisableEntityComponent>(implementor as IDisableEntityComponent));
                    if (implementor is IEnableEntityComponent)
                        _enablingImplementors.Add(new DataStructures.WeakReference<IEnableEntityComponent>(implementor as IEnableEntityComponent));

                    var interfaces = implementor.GetType().GetInterfaces();
                    var weakReference = new DataStructures.WeakReference<object>(implementor);
                    
                    for (int iindex = 0; iindex < interfaces.Length; iindex++)
                    {
                        var componentType = interfaces[iindex];
                        
                         _implementorsByType[componentType] = weakReference;
#if DEBUG && !PROFILER
                        if (_implementorCounterByType.ContainsKey(componentType) == false)
                            _implementorCounterByType[componentType] = 1;
                        else
                            _implementorCounterByType[componentType]++;
#endif                        
                    }
                }
#if DEBUG && !PROFILER
                else
                    Utility.Console.LogError(NULL_IMPLEMENTOR_ERROR.FastConcat(ToString()));
#endif
            }
        }

        void SetupImplementors(
            ref BuildNodeCallbackStruct callBackstruct, 
            ITypeSafeList nodes)
        {
            var RemoveEntity = callBackstruct._internalRemove;
            var DisableEntity = callBackstruct._internalDisable;
            var EnableEntity = callBackstruct._internalEnable;
            
            Action removeEntityAction = () => { RemoveEntity(nodes); nodes.Clear(); };
            Action disableEntityAction = () => DisableEntity(nodes);
            Action enableEntityAction = () => EnableEntity(nodes);

            int removingImplementorsCount = _removingImplementors.Count;
            for (int index = 0; index < removingImplementorsCount; index++)
                _removingImplementors[index].Target.removeEntity = removeEntityAction;
            
            int disablingImplementorsCount = _disablingImplementors.Count;
            for (int index = 0; index < disablingImplementorsCount; index++)
                _disablingImplementors[index].Target.disableEntity = disableEntityAction;
            
            int enablingImplementorsCount = _enablingImplementors.Count;
            for (int index = 0; index < enablingImplementorsCount; index++)
                _enablingImplementors[index].Target.enableEntity = enableEntityAction;
        }

        TNode FillNode<TNode>(TNode node, FillNodeMode mode) where TNode : INode
        {
            var fields = node.GetType().GetFields(BindingFlags.Public |
                                                  BindingFlags.Instance);

            for (int i = fields.Length - 1; i >= 0; --i)
            {
                var field = fields[i];
                Type fieldType = field.FieldType;
                DataStructures.WeakReference<object> component;
                
                if (_implementorsByType.TryGetValue(fieldType, out component) == false)
                {
                    if (mode == FillNodeMode.Strict)
                    {
                        Exception e =
                            new Exception(NOT_FOUND_EXCEPTION +
                                          field.FieldType.Name + " - Node: " + node.GetType().Name +
                                          " - EntityDescriptor " + this);

                        throw e;
                    }
                }
                else
                    field.SetValue(node, component.Target);
                
#if DEBUG && !PROFILER
                {
                    if (_implementorCounterByType[fieldType] != 1)
                    {
                        Utility.Console.LogError(
                            DUPLICATE_IMPLEMENTOR_ERROR.FastConcat("component: ", fieldType.ToString(),
                                                                   " implementor: ", component.Target.ToString()));
                    }
                }
#endif

            }

            return node;
        }

        readonly FasterList<DataStructures.WeakReference<IDisableEntityComponent>> _disablingImplementors = new FasterList<DataStructures.WeakReference<IDisableEntityComponent>>();
        readonly FasterList<DataStructures.WeakReference<IRemoveEntityComponent>>  _removingImplementors = new FasterList<DataStructures.WeakReference<IRemoveEntityComponent>>();
        readonly FasterList<DataStructures.WeakReference<IEnableEntityComponent>>  _enablingImplementors = new FasterList<DataStructures.WeakReference<IEnableEntityComponent>>();

        readonly Dictionary<Type, DataStructures.WeakReference<object>>  _implementorsByType = new Dictionary<Type, DataStructures.WeakReference<object>>();
#if DEBUG && !PROFILER        
        readonly Dictionary<Type, int>  _implementorCounterByType = new Dictionary<Type, int>();
#endif        
        readonly FasterList<INodeBuilder> _nodesToBuild;       

        const string DUPLICATE_IMPLEMENTOR_ERROR = "the same component is implemented with more than one implementor. This is considered an error and MUST be fixed. ";
        const string NULL_IMPLEMENTOR_ERROR = "Null implementor, are you using a wild GetComponents<Monobehaviour> to fetch it? ";
        const string NOT_FOUND_EXCEPTION = "Svelto.ECS: Implementor not found for a Node. Implementor Type: ";
    }
}
