using System;
using System.Reflection;
using UnityEngine;

namespace Svelto.ES
{
    public abstract class BaseNodeHolder<NodeType>:MonoBehaviour, INodeHolder where NodeType : INode
    {
        public INode node { get { if (_node != null) return _node; else return _node = ReturnNode(); } }

        public INodeEnginesRoot engineRoot { set { _engineRoot = value; } }

        protected abstract NodeType ReturnNode();

        void Start()
        {
            if (_engineRoot != null)
                _engineRoot.Add(node);
        }

        NodeType     _node;
        INodeEnginesRoot _engineRoot;
    }

    public abstract class UnityNodeHolder<NodeType>:BaseNodeHolder<NodeType> where NodeType : INode
    {
        protected abstract NodeType GenerateNode();

        override protected NodeType ReturnNode()
        {
            NodeType node = GenerateNode();

            FieldInfo[] fields = typeof(NodeType).GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (int i = fields.Length - 1; i >=0 ; --i)
            {
                var field = fields[i];

                var component = transform.GetComponentsInChildren(field.FieldType, true); //can't use inactive components

                if (component.Length == 0)
                {
                    Exception e = new Exception("Svelto.ES: An Entity must hold all the components needed for a Node. Type: " + field.FieldType.Name + "Entity name: " + name);

                    Debug.LogException(e, gameObject);

                    throw e;
                }
                if (component.Length > 1)
                {
                    Exception e = new Exception("Svelto.ES: An Entity can hold only one component of the same type. Type: " + field.FieldType.Name + "Entity name: " + name);

                    Debug.LogException(e, gameObject);

                    throw e;
                }

                field.SetValue(node, component[0]);
            }

            return node;
        }
    }
}
