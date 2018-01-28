#if UNITY_5 || UNITY_5_3_OR_NEWER
namespace Svelto.ECS
{
    public class GenericEntityDescriptorHolder<T>: 
        UnityEngine.MonoBehaviour , IEntityDescriptorHolder
            where T: class, IEntityDescriptor, new()
    {
        public IEntityDescriptorInfo RetrieveDescriptor()
        {
            return EntityDescriptorTemplate<T>.Default;
        }
    }
}
#endif