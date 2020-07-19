using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.Common;
using Svelto.ECS.Serialization;

namespace Svelto.ECS
{
    partial class EnginesRoot
    {
        sealed class SerializationDescriptorMap
        {
            /// <summary>
            /// Here we want to register all the EntityDescriptors that need to be serialized for network play.
            ///
            /// Remember! This needs to in sync across different clients and server as the values are serialized across
            /// the network also want this to not change so we can save to a DB
            /// </summary>
            internal SerializationDescriptorMap()
            {
                _descriptors = new Dictionary<uint, ISerializableEntityDescriptor>();
                _factories = new Dictionary<uint, IDeserializationFactory>();

                using (new StandardProfiler("Assemblies Scan"))
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                //    Assembly executingAssembly = Assembly.GetExecutingAssembly();

                    Type d1 = typeof(DefaultVersioningFactory<>);
                    foreach (Assembly assembly in assemblies)
                    {
                   //     if (assembly.GetReferencedAssemblies().Contains(executingAssembly.GetName()))
                        {
                            foreach (Type type in GetTypesSafe(assembly))
                            {
                                if (type != null && type.IsClass && type.IsAbstract == false && type.BaseType != null
                                 && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition()
                                 == typeof(SerializableEntityDescriptor<>))
                                {
                                    var descriptor = Activator.CreateInstance(type) as ISerializableEntityDescriptor;

                                    RegisterEntityDescriptor(descriptor, type, d1);
                                }
                            }
                        }
                    }
                }
            } 

            static IEnumerable<Type> GetTypesSafe(Assembly assembly)
            {
                try
                {
                    Type[] types = assembly.GetTypes();

                    return types;
                }
                catch (ReflectionTypeLoadException e)
                {
                    return e.Types;
                }
            }

            void RegisterEntityDescriptor(ISerializableEntityDescriptor descriptor, Type type, Type d1)
            {
                if (descriptor == null)
                {
                    return;
                }

                uint descriptorHash = descriptor.hash;

#if DEBUG && !PROFILE_SVELTO
                if (_descriptors.ContainsKey(descriptorHash))
                {
                    throw new Exception($"Hash Collision of '{descriptor.GetType()}' against " +
                                        $"'{_descriptors[descriptorHash]} ::: {descriptorHash}'");
                }
#endif

                _descriptors[descriptorHash] = descriptor;
                Type[] typeArgs = {type};
                var makeGenericType = d1.MakeGenericType(typeArgs);
                var instance = Activator.CreateInstance(makeGenericType);
                _factories.Add(descriptorHash,  instance as IDeserializationFactory);
            }

            public ISerializableEntityDescriptor GetDescriptorFromHash(uint descriptorID)
            {
#if DEBUG && !PROFILE_SVELTO
                DBC.ECS.Check.Require(_descriptors.ContainsKey(descriptorID),
                    $"Could not find descriptor with ID '{descriptorID}'!");
#endif

                return _descriptors[descriptorID];
            }

            public IDeserializationFactory GetSerializationFactory(uint descriptorID)
            {
                return _factories[descriptorID];
            }

            public void RegisterSerializationFactory<Descriptor>(IDeserializationFactory deserializationFactory)
                where Descriptor : ISerializableEntityDescriptor, new()
            {
                _factories[SerializationEntityDescriptorTemplate<Descriptor>.hash] = deserializationFactory;
            }


            readonly Dictionary<uint, ISerializableEntityDescriptor> _descriptors;
            readonly Dictionary<uint, IDeserializationFactory> _factories;
        }

        /// <summary>
        /// The map of serializable entity hashes to the serializable entity builders (to know the entity structs
        /// to serialize)
        /// </summary>
        SerializationDescriptorMap serializationDescriptorMap { get; }
    }
}
