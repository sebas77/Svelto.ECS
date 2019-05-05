#if UNITY_5 || UNITY_5_3_OR_NEWER
using UnityEngine;

namespace Svelto.ECS.Unity
{
    public static class SveltoGUIHelper
    {
        public static T CreateFromPrefab<T>(ref uint startIndex, Transform contextHolder, IEntityFactory factory, ExclusiveGroup group) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holder = Create<T>(new EGID(startIndex++, group), contextHolder, factory);
            var childs = contextHolder.GetComponentsInChildren<IEntityDescriptorHolder>(true);
        
            foreach (var child in childs)
            {
                if (child.GetType() != typeof(T))
                {
                    var childImplementors = (child as MonoBehaviour).GetComponents<IImplementor>();
                    startIndex = InternalBuildAll(startIndex, child, factory, group, childImplementors);
                }
            }
        
            return holder;
        }
        
        public static T Create<T>(EGID ID, Transform contextHolder,
            IEntityFactory factory) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holder = contextHolder.GetComponentInChildren<T>(true);
            var implementors = holder.GetComponents<IImplementor>();

            factory.BuildEntity(ID, holder.GetDescriptor(), implementors);

            return holder;
        }
        
        public static uint CreateAll<T>(uint startIndex, ExclusiveGroup group, Transform contextHolder,
            IEntityFactory factory) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holders = contextHolder.GetComponentsInChildren<T>(true);

            foreach (var holder in holders)
            {
                var implementors = holder.GetComponents<IImplementor>();

                startIndex = InternalBuildAll(startIndex, holder, factory, group, implementors);
            }

            return startIndex;
        }
        
        static uint InternalBuildAll(uint startIndex, IEntityDescriptorHolder descriptorHolder, IEntityFactory factory, ExclusiveGroup group, IImplementor[] implementors)
        {
            ExclusiveGroup.ExclusiveGroupStruct realGroup = group;
                
            if (string.IsNullOrEmpty(descriptorHolder.groupName) == false)
                realGroup = ExclusiveGroup.Search(descriptorHolder.groupName);

            EGID egid;
            var holderId = descriptorHolder.id;
            if (holderId == 0)
                egid = new EGID(startIndex++, realGroup);
            else
                egid = new EGID(holderId, realGroup);
            
            var init = factory.BuildEntity(egid, descriptorHolder.GetDescriptor(), implementors);
                 
            init.Init(new EntityHierarchyStruct(group));
            
            return startIndex;
        }
    }
}  
#endif