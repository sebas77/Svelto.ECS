using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public class EntityDescriptor
    {
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
             Dictionary<int, Dictionary<Type, ITypeSafeList>> groupNodesByType, 
             ref BuildNodeCallbackStruct callBackstruct)
        {
            for (int index = 0; index < _nodesToBuild.Count; index++)
            {
                var nodeBuilder = _nodesToBuild[index];
                var nodeType = nodeBuilder.GetNodeType();
                
                Dictionary<Type, ITypeSafeList> groupedNodesTyped;
                  
                if (groupNodesByType.TryGetValue(groupID, out groupedNodesTyped) == false)
                {
                    groupedNodesTyped = new Dictionary<Type, ITypeSafeList>(); 
                      
                    groupNodesByType.Add(groupID, groupedNodesTyped);
                };
                
                BuildAndFillNode(entityID, groupedNodesTyped, nodeType, nodeBuilder);
            }
            
            SetupImplementors(ref callBackstruct, entityID);
        }

        internal void BuildNodes(int entityID, 
             Dictionary<Type, ITypeSafeList> nodesByType, 
             ref BuildNodeCallbackStruct callBackstruct)
        {
            int count = _nodesToBuild.Count;
            
            for (int index = 0; index < count; index++)
            {
                var nodeBuilder = _nodesToBuild[index];
                var nodeType = nodeBuilder.GetNodeType();
                
                BuildAndFillNode(entityID, nodesByType, nodeType, nodeBuilder);
            }
            
            SetupImplementors(ref callBackstruct, entityID);
        }
        
        void BuildAndFillNode(int entityID, Dictionary<Type, ITypeSafeList> groupedNodesTyped, Type nodeType, INodeBuilder nodeBuilder)
        {
            ITypeSafeList nodes;

            var nodesPoolWillBeCreated = groupedNodesTyped.TryGetValue(nodeType, out nodes) == false;
            var nodeObjectToFill = nodeBuilder.BuildNodeAndAddToList(ref nodes, entityID);

            if (nodesPoolWillBeCreated)
                groupedNodesTyped.Add(nodeType, nodes);

            //the semantic of this code must still be improved
            //but only classes can be filled, so I am aware
            //it's a NodeWithID
            if (nodeObjectToFill != null)
                FillNode(nodeObjectToFill as NodeWithID);
        }
        
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
            int entityID)
        {
            var RemoveEntity = callBackstruct.internalRemove;
            var DisableEntity = callBackstruct.internalDisable;
            var EnableEntity = callBackstruct.internalEnable;
            
            int removingImplementorsCount = _removingImplementors.Count;
            if (removingImplementorsCount > 0)
            {
                Action removeEntityAction = () => RemoveEntity(_nodesToBuild, entityID);
                
                for (int index = 0; index < removingImplementorsCount; index++)
                    _removingImplementors[index].Target.removeEntity = removeEntityAction;
            }

            int disablingImplementorsCount = _disablingImplementors.Count;
            if (disablingImplementorsCount > 0)
            {
                Action disableEntityAction = () => DisableEntity(_nodesToBuild, entityID);
                
                for (int index = 0; index < disablingImplementorsCount; index++)
                    _disablingImplementors[index].Target.disableEntity = disableEntityAction;
            }

            int enablingImplementorsCount = _enablingImplementors.Count;
            if (enablingImplementorsCount > 0)
            {
                Action enableEntityAction = () => EnableEntity(_nodesToBuild, entityID);
                
                for (int index = 0; index < enablingImplementorsCount; index++)
                    _enablingImplementors[index].Target.enableEntity = enableEntityAction;
            }
        }

        void FillNode<TNode>(TNode node) where TNode : NodeWithID
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
                        Exception e =
                            new Exception(NOT_FOUND_EXCEPTION +
                                          field.FieldType.Name + " - Node: " + node.GetType().Name +
                                          " - EntityDescriptor " + this);

                        throw e;
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
