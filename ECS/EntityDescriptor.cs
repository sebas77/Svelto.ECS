using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;
#if NETFX_CORE
using BindingFlags = System.Reflection.BindingFlags;
#endif

namespace Svelto.ECS
{
    public class EntityDescriptor
    {
        protected EntityDescriptor()
        {
        }
        protected EntityDescriptor(INodeBuilder[] nodesToBuild)
        {
            _nodesToBuild = new FasterList<INodeBuilder>(nodesToBuild);
        }
        protected EntityDescriptor(INodeBuilder[] nodesToBuild, params object[] componentsImplementor):this(nodesToBuild)
        {
            ProcessImplementors(componentsImplementor);
        }

        public void AddImplementors(params object[] componentsImplementor)
        {
            ProcessImplementors(componentsImplementor);
        }

        void ProcessImplementors(object[] implementors)
        {
            for (int index = 0; index < implementors.Length; index++)
            {
                var implementor = implementors[index];
                if (implementor == null)
                {
                    Utility.Console.LogWarning(
                "Null implementor, are you using a wild GetComponents<Monobehaviour> to fetch it? "
                           .FastConcat(ToString()));
                }
                else
                {
                    if (implementor is IRemoveEntityComponent)
                        _removingImplementors.Add(implementor as IRemoveEntityComponent);
                    if (implementor is IDisableEntityComponent)
                        _disablingImplementors.Add(implementor as IDisableEntityComponent);
                    if (implementor is IEnableEntityComponent)
                        _enablingImplementors.Add(implementor as IEnableEntityComponent);

                    var interfaces = implementor.GetType().GetInterfaces();
                    for (int iindex = 0; iindex < interfaces.Length; iindex++)
                    {
                        _implementorsByType[interfaces[iindex]] = implementor;
                    }
                }
            }
        }

        public void AddNodes(params INodeBuilder[] nodesWithID)
        {
            _nodesToBuild.AddRange(nodesWithID);
        }

        public virtual FasterList<INode> BuildNodes(int ID)
        {
            var nodes = new FasterList<INode>();

            for (int index = 0; index < _nodesToBuild.Count; index++)
            {
                var nodeBuilder = _nodesToBuild[index];
                var node = nodeBuilder.Build(ID);

                if (nodeBuilder.reflects != FillNodeMode.None)
                    node = FillNode(node, nodeBuilder.reflects);

                nodes.Add(node);
            }

            return nodes;
        }

        internal FasterList<INode> BuildNodes(int ID,
            Action<FasterList<INode>> removeEntity,
            Action<FasterList<INode>> enableEntity,
            Action<FasterList<INode>> disableEntity)
        {
            var nodes = BuildNodes(ID);

            SetupImplementors(removeEntity, enableEntity, disableEntity, nodes);

            return nodes;
        }

        void SetupImplementors(
            Action<FasterList<INode>> removeEntity, 
            Action<FasterList<INode>> enableEntity, 
            Action<FasterList<INode>> disableEntity, 
            FasterList<INode> nodes)
        {
            Action removeEntityAction = () => { removeEntity(nodes); nodes.Clear(); };
            Action disableEntityAction = () => disableEntity(nodes);
            Action enableEntityAction = () => enableEntity(nodes); 

            for (int index = 0; index < _removingImplementors.Count; index++)
                _removingImplementors[index].removeEntity = removeEntityAction;
            for (int index = 0; index < _disablingImplementors.Count; index++)
                _disablingImplementors[index].disableEntity = disableEntityAction;
            for (int index = 0; index < _enablingImplementors.Count; index++)
                _enablingImplementors[index].enableEntity = enableEntityAction;
        }

        TNode FillNode<TNode>(TNode node, FillNodeMode mode) where TNode : INode
        {
            var fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (int i = fields.Length - 1; i >= 0; --i)
            {
                var field = fields[i];
                Type fieldType = field.FieldType;
                object component;
               
                if ((_implementorsByType.TryGetValue(fieldType, out component)) == false)
                {
                    if (mode == FillNodeMode.Strict)
                    {
                        Exception e =
                            new Exception("Svelto.ECS: Implementor not found for a Node. " + "Implementor Type: " +
                                          field.FieldType.Name + " - Node: " + node.GetType().Name +
                                          " - EntityDescriptor " + this);

                        throw e;
                    }
                }
                else
                    field.SetValue(node, component);
            }

            return node;
        }

        readonly FasterList<IDisableEntityComponent> _disablingImplementors = new FasterList<IDisableEntityComponent>();
        readonly FasterList<IRemoveEntityComponent>  _removingImplementors = new FasterList<IRemoveEntityComponent>();
        readonly FasterList<IEnableEntityComponent>  _enablingImplementors = new FasterList<IEnableEntityComponent>();
        readonly Dictionary<Type, object> _implementorsByType = new Dictionary<Type, object>();

        readonly FasterList<INodeBuilder> _nodesToBuild;
    }

    public interface INodeBuilder
    {
        INode Build(int ID);

        FillNodeMode reflects { get; }
    }

    public class NodeBuilder<NodeType> : INodeBuilder where NodeType : NodeWithID, new()
    {
        public INode Build(int ID)
        {
            NodeWithID node = NodeWithID.BuildNode<NodeType>(ID);

            return (NodeType)node;
        }

        public FillNodeMode reflects { get { return FillNodeMode.Strict; } }
    }

    public class StructNodeBuilder<NodeType> : INodeBuilder
        where NodeType : struct, IStructNodeWithID
    {
        public INode Build(int ID)
        {
            var shortID = (short)ID;
            IStructNodeWithID node = default(NodeType);
            node.ID = shortID;

            return node;
        }

        public virtual FillNodeMode reflects { get { return FillNodeMode.Relaxed; } }
    }

    public class FastStructNodeBuilder<NodeType> : StructNodeBuilder<NodeType>
        where NodeType : struct, IStructNodeWithID
    {
        public override FillNodeMode reflects { get { return FillNodeMode.None; } }
    }

    public enum FillNodeMode
    {
        Strict,
        Relaxed,

        None
    }
}
