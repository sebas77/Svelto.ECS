#if UNITY_5 || UNITY_5_3_OR_NEWER
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    public static class SveltoGUIHelper
    {
        public static T CreateFromPrefab<T>(ref uint startIndex, Transform contextHolder, IEntityFactory factory,
            ExclusiveGroup group, string groupNamePostfix = null) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holder = Create<T>(new EGID(startIndex++, group), contextHolder, factory);
            var childs = contextHolder.GetComponentsInChildren<IEntityDescriptorHolder>(true);

            foreach (var child in childs)
            {
                if (child.GetType() != typeof(T))
                {
                    var monoBehaviour = child as MonoBehaviour;
                    var childImplementors = monoBehaviour.GetComponents<IImplementor>();
                    startIndex = InternalBuildAll(
                        startIndex,
                        child,
                        factory,
                        group,
                        childImplementors,
                        groupNamePostfix);
                }
            }

            return holder;
        }


        public static T Create<T>(EGID ID, Transform contextHolder, IEntityFactory factory)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holder = contextHolder.GetComponentInChildren<T>(true);
            DBC.ECS.Check.Assert(holder != null, $"`{nameof(holder)}` is null! No component of type " +
                                                 $"`{typeof(T)}` was found between its children.");

            var implementors = holder.GetComponents<IImplementor>();

            factory.BuildEntity(ID, holder.GetDescriptor(), implementors);

            return holder;
        }

        public static EntityComponentInitializer CreateWithEntity<T>(EGID ID, Transform contextHolder,
            IEntityFactory factory, out T holder)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            holder = contextHolder.GetComponentInChildren<T>(true);
            var implementors = holder.GetComponents<IImplementor>();

            return factory.BuildEntity(ID, holder.GetDescriptor(), implementors);
        }

        public static uint CreateAll<T>(uint startIndex, ExclusiveGroup group,
            Transform contextHolder, IEntityFactory factory, string groupNamePostfix = null) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holders = contextHolder.GetComponentsInChildren<T>(true);

            foreach (var holder in holders)
            {
                var implementors = holder.GetComponents<IImplementor>();

                startIndex = InternalBuildAll(startIndex, holder, factory, group, implementors, groupNamePostfix);
            }

            return startIndex;
        }

        static uint InternalBuildAll(uint startIndex, IEntityDescriptorHolder descriptorHolder,
            IEntityFactory factory, ExclusiveGroup group, IImplementor[] implementors, string groupNamePostfix)
        {
            ExclusiveGroupStruct realGroup = group;

            if (string.IsNullOrEmpty(descriptorHolder.groupName) == false)
            {
                realGroup = ExclusiveGroup.Search(!string.IsNullOrEmpty(groupNamePostfix)
                    ? $"{descriptorHolder.groupName}{groupNamePostfix}"
                    : descriptorHolder.groupName);
            }

            EGID egid;
            var holderId = descriptorHolder.id;
            if (holderId == 0)
                egid = new EGID(startIndex++, realGroup);
            else
                egid = new EGID(holderId, realGroup);

            var init = factory.BuildEntity(egid, descriptorHolder.GetDescriptor(), implementors);

            init.Init(new EntityHierarchyComponent(group));

            return startIndex;
        }

      /// <summary>
        /// Works like CreateAll but only builds entities with holders that have the same group specfied
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="group">The group to match</param>
        /// <param name="contextHolder"></param>
        /// <param name="factory"></param>
        /// <typeparam name="T">EntityDescriptorHolder type</typeparam>
        /// <returns>Next available ID</returns>
        public static uint CreateAllInMatchingGroup<T>(uint startId, ExclusiveGroup exclusiveGroup,
            Transform contextHolder, IEntityFactory factory) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holders = contextHolder.GetComponentsInChildren<T>(true);

            foreach (var holder in holders)
            {
                if (string.IsNullOrEmpty(holder.groupName) == false)
                {
                    var realGroup = ExclusiveGroup.Search(holder.groupName);
                    if (realGroup != exclusiveGroup)
                        continue;
                }
                else
                {
                    continue;
                }

                var implementors = holder.GetComponents<IImplementor>();

                startId = InternalBuildAll(startId, holder, factory, exclusiveGroup, implementors, null);
            }

            return startId;
        }
    }
}
#endif
