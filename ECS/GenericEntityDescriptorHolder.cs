#if UNITY_5 || UNITY_5_3_OR_NEWER
using System;

namespace Svelto.ECS
{
    public class GenericEntityDescriptorHolder<T, I>: 
        UnityEngine.MonoBehaviour, IEntityDescriptorHolder where T:EntityDescriptor
    {
        public EntityDescriptor BuildDescriptorType(object[] externalImplentors)
        {
            I[] implementors;

            if (externalImplentors != null)
            {
                I[] baseImplentors = gameObject.GetComponents<I>();

                implementors = new I[externalImplentors.Length + baseImplentors.Length];

                Array.Copy(baseImplentors, implementors, baseImplentors.Length);
                Array.Copy(externalImplentors, 0, implementors, baseImplentors.Length, externalImplentors.Length);
            }
            else
            {
                implementors = gameObject.GetComponents<I>();
            }

            return (T)Activator.CreateInstance(typeof(T), implementors);
        }
    }
}
#endif