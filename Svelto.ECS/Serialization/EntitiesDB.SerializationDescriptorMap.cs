using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.Common;
using Svelto.ECS.Serialization;

namespace Svelto.ECS
{
    partial class EnginesRoot
    {
        /// <summary>
        /// The map of serializable entity hashes to the serializable entity builders (to know the entity structs
        /// to serialize)
        /// </summary>
        readonly SerializationDescriptorMap _serializationDescriptorMap;

        sealed class SerializationDescriptorMap
        {
            static readonly Dictionary<uint, ISerializableEntityDescriptor> _descriptors;
            static readonly Dictionary<uint, IDeserializationFactory>       _defaultFactories;

            static SerializationDescriptorMap()
            {
                _descriptors      = new Dictionary<uint, ISerializableEntityDescriptor>();
                _defaultFactories = new Dictionary<uint, IDeserializationFactory>();
            }

            /// <summary>
            /// Here we want to register all the EntityDescriptors that need to be serialized for network play.
            ///
            /// Remember! This needs to in sync across different clients and server as the values are serialized across
            /// the network also want this to not change so we can save to a DB
            /// </summary>
            internal SerializationDescriptorMap()
            {
                _factories = new Dictionary<uint, IDeserializationFactory>();
                
                CopyDefaultFactories();
            }

            public ISerializableEntityDescriptor GetDescriptorFromHash(uint descriptorHash)
            {
#if DEBUG && !PROFILE_SVELTO
                DBC.ECS.Check.Require(_descriptors.ContainsKey(descriptorHash)
                                    , $"Could not find descriptor linked to hash, wrong deserialization size? '{descriptorHash}'!");
#endif

                return _descriptors[descriptorHash];
            }

            public IDeserializationFactory GetSerializationFactory(uint descriptorHash)
            {
#if DEBUG && !PROFILE_SVELTO
                DBC.ECS.Check.Require(_descriptors.ContainsKey(descriptorHash)
                                    , $"Could not find descriptor linked to descriptor hash, wrong deserialization size? '{descriptorHash}'!");
                DBC.ECS.Check.Require(_factories.ContainsKey(descriptorHash)
                                    , $"Could not find factory linked to hash '{_descriptors[descriptorHash]}'!");
#endif
                return _factories[descriptorHash];
            }

            public void RegisterSerializationFactory<Descriptor>(IDeserializationFactory deserializationFactory)
                where Descriptor : ISerializableEntityDescriptor, new()
            {
                _factories[SerializationEntityDescriptorTemplate<Descriptor>.hash] = deserializationFactory;
            }

            void CopyDefaultFactories()
            {
                foreach (var factory in _defaultFactories)
                {
                    _factories[factory.Key] = factory.Value;
                }
            }

            internal static void Init()
            {
                using (new StandardProfiler("Assemblies Scan"))
                {
                    List<Assembly> assemblies = AssemblyUtility.GetCompatibleAssemblies();

                    Type defaultFactory = typeof(DefaultVersioningFactory<>);
                    foreach (Assembly assembly in assemblies)
                    {
                        foreach (Type type in AssemblyUtility.GetTypesSafe(assembly))
                        {
                            if (type != null && type.IsClass && type.IsAbstract == false && type.BaseType != null
                             && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition()
                             == typeof(SerializableEntityDescriptor<>))
                            {
                                var descriptor = Activator.CreateInstance(type) as ISerializableEntityDescriptor;

                                RegisterEntityDescriptor(descriptor, type, defaultFactory);
                            }
                        }
                    }
                }
            }

            static void RegisterEntityDescriptor(ISerializableEntityDescriptor descriptor, Type type, Type d1)
            {
                if (descriptor == null)
                    return;

                uint descriptorHash = descriptor.hash;

#if DEBUG && !PROFILE_SVELTO
                if (_descriptors.ContainsKey(descriptorHash))
                {
                    throw new Exception($"Hash Collision of '{descriptor.GetType()}' against "
                                      + $"'{_descriptors[descriptorHash]} ::: {descriptorHash}'");
                }
#endif
                _descriptors[descriptorHash] = descriptor;
                Type[] typeArgs        = { type };
                var    makeGenericType = d1.MakeGenericType(typeArgs);
                var    instance        = Activator.CreateInstance(makeGenericType);
                _defaultFactories.Add(descriptorHash, instance as IDeserializationFactory);
            }

            readonly        Dictionary<uint, IDeserializationFactory>       _factories;
        }
    }
}