using System;
using System.Collections.Generic;
using System.Reflection;
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

                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    foreach (Type type in GetTypesSafe(assembly))
                    {
                        if (type != null && type.IsClass && type.GetConstructor(Type.EmptyTypes) != null &&
                            typeof(ISerializableEntityDescriptor).IsAssignableFrom(type))
                        {
                            var descriptor = Activator.CreateInstance(type) as ISerializableEntityDescriptor;

                            RegisterEntityDescriptor(descriptor);
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

            void RegisterEntityDescriptor(ISerializableEntityDescriptor descriptor)
            {
                if (descriptor == null)
                {
                    return;
                }

                uint descriptorHash = descriptor.hash;

#if DEBUG && !PROFILER
                if (_descriptors.ContainsKey(descriptorHash))
                {
                    throw new Exception($"Hash Collision of '{descriptor.GetType()}' against " +
                                        $"'{_descriptors[descriptorHash]} ::: {descriptorHash}'");
                }
#endif

                _descriptors[descriptorHash] = descriptor;
            }

            public ISerializableEntityDescriptor GetDescriptorFromHash(uint descriptorID)
            {
#if DEBUG && !PROFILER
                DBC.ECS.Check.Require(_descriptors.ContainsKey(descriptorID),
                    $"Could not find descriptor with ID '{descriptorID}'!");
#endif

                return _descriptors[descriptorID];
            }

            public IDeserializationFactory GetSerializationFactory(uint descriptorID)
            {
                return _factories.TryGetValue(descriptorID, out var factory) ? factory : null;
            }

            public void RegisterSerializationFactory<Descriptor>(IDeserializationFactory deserializationFactory)
                where Descriptor : ISerializableEntityDescriptor, new()
            {
                _factories.Add(SerializationEntityDescriptorTemplate<Descriptor>.hash, deserializationFactory);
            }


            readonly Dictionary<uint, ISerializableEntityDescriptor> _descriptors;
            readonly Dictionary<uint, IDeserializationFactory> _factories;
        }

        /// <summary>
        /// The map of serializable entity hashes to the serializable entity builders (to know the entity structs
        /// to serialize)
        /// </summary>
        SerializationDescriptorMap serializationDescriptorMap { get; } = new SerializationDescriptorMap();
    }
}
