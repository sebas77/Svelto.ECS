#if UNITY_5 || UNITY_5_3_OR_NEWER
using UnityEngine;

namespace Svelto.ECS.Unity
{
    public static class SveltoEntityFactoryForUnity
    {
        public static T Create<T>(EGID ID, Transform contextHolder,
            IEntityFactory factory) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holder = contextHolder.GetComponentInChildren<T>(true);
            var implementors = holder.GetComponents<IImplementor>();

            factory.BuildEntity(ID, holder.GetDescriptor(), implementors);

            return holder;
        }
        
        public static void CreateAll<T>(ExclusiveGroup group, Transform contextHolder,
            IEntityFactory factory) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holders       = contextHolder.GetComponentsInChildren<T>(true);

            foreach (var holder in holders)
            {
                var implementors = holder.GetComponents<IImplementor>();

                ExclusiveGroup.ExclusiveGroupStruct realGroup = group;

                if (string.IsNullOrEmpty( holder.groupName) == false)
                    realGroup = ExclusiveGroup.Search(holder.groupName);

                factory.BuildEntity(holder.GetInstanceID(), realGroup, holder.GetDescriptor(), implementors);
            }
        }
    }
}  
#endif