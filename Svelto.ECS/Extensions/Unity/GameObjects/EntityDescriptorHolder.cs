#if UNITY_5 || UNITY_5_3_OR_NEWER
using System;
using System.Collections.Generic;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    public class ListToPopupAttribute : PropertyAttribute
    {
        public Type   classType;
        public string listName;
    
        public ListToPopupAttribute(Type classType, string listName)
        {
            this.classType = classType;
            this.listName  = listName;
        }
    }
    /// <summary>
    /// I introduced this option thinking it could be a good idea, but I am not sure anymore. Although it's slightly
    /// more annoying, extending GenericEntityDescriptorHolder is wiser than using this class.
    /// Consider this experimental
    /// Todo: sort in alphabetic order
    /// Todo: hide inner descriptors
    /// </summary>
    public class EntityDescriptorHolder : MonoBehaviour, IEntityDescriptorHolder
    {
        public IEntityDescriptor GetDescriptor() { return type; }

        public string groupName => _groupName;
        public ushort id        => _id;

        [Tooltip(
            "it's possible to name groups and query group by name. This entity will be created in a named group if inserted")]
        [SerializeField]
        string _groupName;

        [Tooltip("this entity will be created with the selected ID, if inserted. An ID must be unique in each group")]
        [SerializeField]
        ushort _id;

        [Tooltip("choose the entity type, not optional")]
        [ListToPopup(typeof(EntityDescriptorHolder), "DescriptorList")]
        [SerializeField]
        string Descriptor;

        internal IEntityDescriptor type;

        static List<Type> DescriptorList = new List<Type>();

        static EntityDescriptorHolder()
        {
            var  assemblies = AssemblyUtility.GetCompatibleAssemblies();
            Type d1         = typeof(IEntityDescriptor);

            foreach (var assembly in assemblies)
            foreach (Type type in AssemblyUtility.GetTypesSafe(assembly))
            {
                if (type != null && d1.IsAssignableFrom(type) && type.IsAbstract == false)
                {
                    DescriptorList.Add(type);
                }
            }
        }
    }
}

#endif