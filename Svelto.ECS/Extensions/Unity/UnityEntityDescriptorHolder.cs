#if UNITY_5 || UNITY_5_3_OR_NEWER
namespace Svelto.ECS
{
    public class UnityEntityDescriptorHolder<T>: 
        UnityEngine.MonoBehaviour , IEntityDescriptorHolder
            where T: IEntityDescriptor, new()
    {
        public IEntityBuilder[] GetEntitiesToBuild()
        {
            return EntityDescriptorTemplate<T>.descriptor.entitiesToBuild;
        }
    }
}
#endif