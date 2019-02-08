#if UNITY_5 || UNITY_5_3_OR_NEWER
using Svelto.Context;
using UnityEngine;

namespace Svelto.ECS.Unity
{
    public static class SveltoEntityFactoryForUnity
    {
        public static void Create<T>(EGID ID, UnityContext   contextHolder,
                                     IEntityFactory factory) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holder = contextHolder.GetComponentInChildren<T>(true);
            var implementors = holder.GetComponents<IImplementor>();

            factory.BuildEntity(ID, holder.GetDescriptor(), implementors);
        }
        
        public static void CreateAll<T>(ExclusiveGroup group, UnityContext contextHolder,
                                     IEntityFactory factory) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holders       = contextHolder.GetComponentsInChildren<T>(true);

            foreach (var holder in holders)
            {
                var implementors = holder.GetComponents<IImplementor>();

                factory.BuildEntity(holder.GetInstanceID(), group, holder.GetDescriptor(), implementors);
            }
        }
    }
}  
#endif