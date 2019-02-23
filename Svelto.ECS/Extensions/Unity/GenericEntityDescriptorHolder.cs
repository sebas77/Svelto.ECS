using UnityEngine;

#if UNITY_5 || UNITY_5_3_OR_NEWER
namespace Svelto.ECS.Unity
{
    public class GenericEntityDescriptorHolder<T>: 
        MonoBehaviour , IEntityDescriptorHolder
            where T: IEntityDescriptor, new()
    {
        public IEntityDescriptor GetDescriptor()
        {
            return EntityDescriptorTemplate<T>.descriptor;
        }

        public string groupName => _groupName;

        [SerializeField]
#pragma warning disable 649        
        string _groupName;
#pragma warning restore 649        
    }
}
#endif