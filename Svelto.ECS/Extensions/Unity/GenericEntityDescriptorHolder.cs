#if UNITY_5 || UNITY_5_3_OR_NEWER
namespace Svelto.ECS.Unity
{
    public class GenericEntityDescriptorHolder<T>: 
        UnityEngine.MonoBehaviour , IEntityDescriptorHolder
            where T: IEntityDescriptor, new()
    {
        public IEntityDescriptor GetDescriptor()
        {
            return EntityDescriptorTemplate<T>.descriptor;
        }
    }
}
#endif