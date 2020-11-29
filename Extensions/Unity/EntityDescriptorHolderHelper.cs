#if UNITY_ECS
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    public static class EntityDescriptorHolderHelper
    {
        public static EntityComponentInitializer CreateEntity<T>(this Transform contextHolder, EGID ID,
                                                                 IEntityFactory factory, out T holder)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            holder = contextHolder.GetComponentInChildren<T>(true);
            var implementors = holder.GetComponents<IImplementor>();

            return factory.BuildEntity(ID, holder.GetDescriptor(), implementors);
        }
        
        public static EntityComponentInitializer Create<T>(this Transform contextHolder, EGID ID,
                                                           IEntityFactory factory)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holder       = contextHolder.GetComponentInChildren<T>(true);
            var implementors = holder.GetComponents<IImplementor>();

            return factory.BuildEntity(ID, holder.GetDescriptor(), implementors);
        }
    }
}
#endif