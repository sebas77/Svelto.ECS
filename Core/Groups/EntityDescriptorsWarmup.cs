using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    static class EntityDescriptorsWarmup
    {
        /// <summary>
        /// c# Static constructors are guaranteed to be thread safe
        /// Warmup all EntityDescriptors and ComponentTypeID classes to avoid huge overheads when they are first used
        /// </summary>
        internal static void WarmUp()
        {
            List<Assembly> assemblies = AssemblyUtility.GetCompatibleAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                var typeOfEntityDescriptors = typeof(IEntityDescriptor);

                foreach (Type type in AssemblyUtility.GetTypesSafe(assembly))
                {
                    if (type.IsInterface == false && type.IsAbstract == false && type.IsGenericType == false && typeOfEntityDescriptors.IsAssignableFrom(type)) 
                    {
                        try
                        {
                            //the main goal of this iteration is to warm up the component builders and descriptors
                            //note: I could have just instanced the entity descriptor, but in this way I will warm up the EntityDescriptorTemplate too
                            var warmup = typeof(EntityDescriptorTemplate<>).MakeGenericType(type);

                            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(warmup.TypeHandle);
                        }
                        catch { }
                    }
                }
                
                var typeOfComponents = typeof(_IInternalEntityComponent);

                //the main goal of this iteration is to warm up the component IDs
                foreach (Type type in AssemblyUtility.GetTypesSafe(assembly))
                {
                    if (type.IsInterface == false && typeOfComponents.IsAssignableFrom(type)) //IsClass and IsSealed and IsAbstract means only static classes
                    {
                        try
                        {
                            var componentType = typeof(ComponentTypeID<>).MakeGenericType(type);
                            //is called only once ever, even if runs multiple times.
                            //this warms up the component builder. There could be different implementation of components builders for the same component type in theory
                            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(componentType.TypeHandle);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}