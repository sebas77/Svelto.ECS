#if UNITY_5 || UNITY_5_3_OR_NEWER
using System;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    // "New Svelto GUI Patterns are now available"
    public static class SveltoGUIHelper
    {
        /// <summary>
        /// This is the suggested way to create GUIs from prefabs now.
        /// </summary>
        public static T CreateFromPrefab<T>
        (ref uint startIndex, Transform contextHolder, IEntityFactory factory, ExclusiveGroup group
       , bool searchImplementorsInChildren = false, string groupNamePostfix = null)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            Create<T>(new EGID(startIndex++, group), contextHolder, factory, out var holder);
            var children = contextHolder.GetComponentsInChildren<IEntityDescriptorHolder>(true);

            foreach (var child in children)
            {
                if (child.GetType() != typeof(T))
                {
                    var            monoBehaviour = child as MonoBehaviour;
                    IImplementor[] childImplementors;
                    if (searchImplementorsInChildren == false)
                        childImplementors = monoBehaviour.GetComponents<IImplementor>();
                    else
                        childImplementors = monoBehaviour.GetComponentsInChildren<IImplementor>(true);

                    startIndex = InternalBuildAll(startIndex, child, factory, group, childImplementors
                                                , groupNamePostfix);
                }
            }

            return holder;
        }

        /// <summary>
        /// Creates all the entities in a hierarchy. This was commonly used to create entities from gameobjects
        /// already present in the scene
        /// </summary>
        public static uint CreateAll<T>
        (uint startIndex, ExclusiveGroup group, Transform contextHolder, IEntityFactory factory
       , string groupNamePostfix = null) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holders = contextHolder.GetComponentsInChildren<T>(true);

            foreach (var holder in holders)
            {
                var implementors = holder.GetComponents<IImplementor>();
                try
                {
                    startIndex = InternalBuildAll(startIndex, holder, factory, group, implementors, groupNamePostfix);
                }
                catch (Exception ex)
                {
                    throw new Exception($"When building entity from game object {Path(holder.transform)}", ex);
                }
            }

            return startIndex;
        }

        public static EntityInitializer Create<T>(EGID ID, Transform contextHolder, IEntityFactory factory, out T holder
                           , bool searchImplementorsInChildren = false) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            holder = contextHolder.GetComponentInChildren<T>(true);
            if (holder == null)
            {
                throw new Exception($"Could not find holder {typeof(T).Name} in {contextHolder.name}");
            }

            var implementors = searchImplementorsInChildren == false
                ? holder.GetComponents<IImplementor>()
                : holder.GetComponentsInChildren<IImplementor>(true);

            return factory.BuildEntity(ID, holder.GetDescriptor(), implementors);
        }

        public static EntityInitializer Create<T>
            (EGID ID, Transform contextHolder, IEntityFactory factory, bool searchImplementorsInChildren = false)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            return Create<T>(ID, contextHolder, factory, out _, searchImplementorsInChildren);
        }

        /// <summary>
        /// Works like CreateAll but only builds entities with holders that have the same group specified
        /// This is a very specific case and I still need to decide if I want it in the framework
        /// </summary>
        public static uint CreateAllInMatchingGroup<T>
            (uint startId, ExclusiveGroup exclusiveGroup, Transform contextHolder, IEntityFactory factory)
            where T : MonoBehaviour, IEntityDescriptorHolder
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

        static string Path(Transform go)
        {
            string s = go.name;
            while (go.parent != null)
            {
                go = go.parent;
                s  = go.name + "/" + s;
            }

            return s;
        }

        static uint InternalBuildAll
        (uint startIndex, IEntityDescriptorHolder descriptorHolder, IEntityFactory factory, ExclusiveGroup group
       , IImplementor[] implementors, string groupNamePostfix)
        {
            ExclusiveGroupStruct realGroup = group;

            if (string.IsNullOrEmpty(descriptorHolder.groupName) == false)
            {
                realGroup = ExclusiveGroup.Search(!string.IsNullOrEmpty(groupNamePostfix)
                                                      ? $"{descriptorHolder.groupName}{groupNamePostfix}"
                                                      : descriptorHolder.groupName);
            }

            EGID egid;
            var  holderId = descriptorHolder.id;
            if (holderId == 0)
                egid = new EGID(startIndex++, realGroup);
            else
                egid = new EGID(holderId, realGroup);

            var init = factory.BuildEntity(egid, descriptorHolder.GetDescriptor(), implementors);

            init.Init(new EntityHierarchyComponent(group));

            return startIndex;
        }
    }
}
#endif