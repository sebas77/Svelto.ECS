#if UNITY_5 || UNITY_5_3_OR_NEWER
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    public abstract class GenericEntityDescriptorHolder<T>: MonoBehaviour, IEntityDescriptorHolder
            where T: IEntityDescriptor, new()
    {
        public IEntityDescriptor GetDescriptor()
        {
            return EntityDescriptorTemplate<T>.descriptor;
        }

        public T GetRealDescriptor()
        {
            return EntityDescriptorTemplate<T>.realDescriptor;
        }
        
        public string groupName => _groupName;
        public ushort id => _id;

#pragma warning disable 649
        [SerializeField] string _groupName;
        [SerializeField] ushort _id;
#pragma warning restore 649
    }
}
#endif