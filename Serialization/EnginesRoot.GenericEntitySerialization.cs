using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.ECS.Serialization;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        sealed class EntitySerialization : IEntitySerialization
        {
            public void SerializeEntity(EGID egid, ISerializationData serializationData, int serializationType)
            {
                var entitiesDb = _enginesRoot._entitiesDB;

                //needs to retrieve the meta data associated with the entity
                ref var serializableEntityComponent = ref entitiesDb.QueryEntity<SerializableEntityComponent>(egid);
                uint    descriptorHash              = serializableEntityComponent.descriptorHash;

                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot._serializationDescriptorMap;
                ISerializableEntityDescriptor entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);
                ISerializableComponentBuilder[] entityComponentsToSerialise = entityDescriptor.componentsToSerialize;

                var header =
                    new SerializableEntityHeader(descriptorHash, egid, (byte)entityComponentsToSerialise.Length);
                header.Copy(serializationData);

                var length = entityComponentsToSerialise.Length;
                for (int index = 0; index < length; index++)
                {
                    var entityBuilder = entityComponentsToSerialise[index];

                    serializationData.BeginNextEntityComponent();
                    SerializeEntityComponent(egid, entityBuilder, serializationData, serializationType);
                }
            }

            public EntityInitializer DeserializeNewEntity(EGID egid, ISerializationData serializationData,
                int serializationType)
            {
                //todo: SerializableEntityHeader may be needed to be customizable
                var serializableEntityHeader = new SerializableEntityHeader(serializationData);

                uint descriptorHash = serializableEntityHeader.descriptorHash;
                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot._serializationDescriptorMap;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);
                IDeserializationFactory factory = serializationDescriptorMap.GetSerializationFactory(descriptorHash);

                return factory.BuildDeserializedEntity(egid, serializationData, entityDescriptor, serializationType,
                    this, _enginesRoot.GenerateEntityFactory(), _enginesRoot is SerializingEnginesRoot);
            }

            public void DeserializeEntity(ISerializationData serializationData, int serializationType)
            {
                var serializableEntityHeader = new SerializableEntityHeader(serializationData);

                EGID egid = serializableEntityHeader.egid;

                DeserializeEntityInternal(serializationData, egid, serializableEntityHeader, serializationType);
            }

            public void DeserializeEntity(EGID egid, ISerializationData serializationData, int serializationType)
            {
                var serializableEntityHeader = new SerializableEntityHeader(serializationData);

                DeserializeEntityInternal(serializationData, egid, serializableEntityHeader, serializationType);
            }

            public void DeserializeEntityComponents(ISerializationData serializationData,
                ISerializableEntityDescriptor entityDescriptor, ref EntityInitializer initializer,
                int serializationType)
            {
                foreach (var serializableEntityBuilder in entityDescriptor.componentsToSerialize)
                {
                    serializationData.BeginNextEntityComponent();
                    serializableEntityBuilder.Deserialize(serializationData, initializer, serializationType);
                }
            }

            /// <summary>
            /// Note this has been left undocumented and forgot over the months. The initial version was obviously
            /// wrong, as it wasn't looking for T but only assuming that T was the first component in the entity.
            /// It's also weird or at least must be revalidated, the fact that serializationData works only as
            /// a tape, so we need to reset datapos in case we do not want to forward the head.
            /// </summary>
            /// <param name="serializationData"></param>
            /// <param name="entityDescriptor"></param>
            /// <param name="serializationType"></param>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public T DeserializeEntityComponent<T>(ISerializationData serializationData,
                ISerializableEntityDescriptor entityDescriptor, int serializationType)
                where T : unmanaged, IEntityComponent
            {
                var readPos         = serializationData.dataPos;
                T   entityComponent = default;

                foreach (var serializableEntityBuilder in entityDescriptor.componentsToSerialize)
                {
                    if (serializableEntityBuilder is SerializableComponentBuilder<T> entityBuilder)
                    {
                        entityBuilder.Deserialize(serializationData, ref entityComponent, serializationType);

                        break;
                    }
                    else
                        serializationData.dataPos += (uint)serializableEntityBuilder.Size(serializationType);
                }

                serializationData.dataPos = readPos;
                return entityComponent;
            }

            public void DeserializeEntityToSwap(EGID fromEGID, EGID toEGID, [CallerMemberName] string caller = null)
            {
                EntitiesDB entitiesDb = _enginesRoot._entitiesDB;
                ref var serializableEntityComponent = ref entitiesDb.QueryEntity<SerializableEntityComponent>(fromEGID);

                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot._serializationDescriptorMap;
                uint descriptorHash = serializableEntityComponent.descriptorHash;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);

                _enginesRoot.CheckSwapEntityID(fromEGID, toEGID, entityDescriptor.realType, caller);

                /// Serializable Entity Descriptors can be extended so we need to use FindRealComponents
                _enginesRoot.QueueSwapEntityOperation(fromEGID, toEGID,
                    _enginesRoot.FindRealComponents(fromEGID, entityDescriptor.componentsToBuild), caller);
            }

            public void DeserializeEntityToDelete(EGID egid, [CallerMemberName] string caller = null)
            {
                EntitiesDB entitiesDB                  = _enginesRoot._entitiesDB;
                ref var    serializableEntityComponent = ref entitiesDB.QueryEntity<SerializableEntityComponent>(egid);
                uint       descriptorHash              = serializableEntityComponent.descriptorHash;

                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot._serializationDescriptorMap;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);

                _enginesRoot.CheckRemoveEntityID(egid, entityDescriptor.realType, caller);

                try
                {
                    /// Serializable Entity Descriptors can be extended so we need to use FindRealComponents
                    _enginesRoot.QueueRemoveEntityOperation(egid,
                        _enginesRoot.FindRealComponents(egid, entityDescriptor.componentsToBuild), caller);
                }
                catch
                {
                    Console.LogError(
                        $"something went wrong while deserializing entity {entityDescriptor.realType}");

                    throw;
                }
            }

            public void SkipEntityDeserialization(ISerializationData serializationData, int serializationType,
                int numberOfEntities)
            {
                uint dataPositionBeforeHeader = serializationData.dataPos;
                var  serializableEntityHeader = new SerializableEntityHeader(serializationData);

                uint headerSize = serializationData.dataPos - dataPositionBeforeHeader;

                uint descriptorHash = serializableEntityHeader.descriptorHash;
                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot._serializationDescriptorMap;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);

                uint componentSizeTotal = 0;

                foreach (var serializableEntityBuilder in entityDescriptor.componentsToSerialize)
                {
                    componentSizeTotal += (uint)serializableEntityBuilder.Size(serializationType);
                }

                //When constructing an SerializableEntityHeader the data position of the serializationData is incremented by the size of the header.
                //Since a header is needed to get the entity descriptor, we need to account for one less header than usual, since the data has already
                //been incremented once.
                var totalBytesToSkip = (uint)(headerSize * (numberOfEntities - 1)) +
                    (uint)(componentSizeTotal * numberOfEntities);

                serializationData.dataPos += totalBytesToSkip;
            }

            public uint GetHashFromGroup(ExclusiveGroupStruct groupStruct)
            {
                return GroupHashMap.GetHashFromGroup(groupStruct);
            }

            public ExclusiveGroupStruct GetGroupFromHash(uint groupHash)
            {
                return GroupHashMap.GetGroupFromHash(groupHash);
            }

            public void RegisterSerializationFactory<T>(IDeserializationFactory deserializationFactory)
                where T : ISerializableEntityDescriptor, new()
            {
                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot._serializationDescriptorMap;
                serializationDescriptorMap.RegisterSerializationFactory<T>(deserializationFactory);
            }

            internal EntitySerialization(EnginesRoot enginesRoot)
            {
                _root = new DataStructures.WeakReference<EnginesRoot>(enginesRoot);
            }

            void SerializeEntityComponent(EGID entityGID, ISerializableComponentBuilder componentBuilder,
                ISerializationData serializationData, int serializationType)
            {
                ExclusiveGroupStruct groupId    = entityGID.groupID;
                var                 entityType = componentBuilder.getComponentID;
                if (!_enginesRoot._entitiesDB.UnsafeQueryEntityDictionary(groupId, entityType, out var safeDictionary))
                {
                    throw new Exception("Entity Serialization failed");
                }

                componentBuilder.Serialize(entityGID.entityID, safeDictionary, serializationData, serializationType);
            }

            void DeserializeEntityInternal(ISerializationData serializationData, EGID egid,
                SerializableEntityHeader serializableEntityHeader, int serializationType)
            {
                SerializationDescriptorMap descriptorMap = _enginesRoot._serializationDescriptorMap;
                var entityDescriptor = descriptorMap.GetDescriptorFromHash(serializableEntityHeader.descriptorHash);

                if (_enginesRoot._groupEntityComponentsDB.TryGetValue(egid.groupID, out var entitiesInGroupPerType) ==
                    false)
                    throw new Exception("Entity Serialization failed");

                foreach (var serializableEntityBuilder in entityDescriptor.componentsToSerialize)
                {
                    entitiesInGroupPerType.TryGetValue(
                        serializableEntityBuilder.getComponentID, out var safeDictionary);

                    serializationData.BeginNextEntityComponent();
                    serializableEntityBuilder.Deserialize(egid.entityID, safeDictionary, serializationData,
                        serializationType);
                }
            }

            EnginesRoot                                               _enginesRoot => _root.Target;
            readonly DataStructures.WeakReference<EnginesRoot> _root;
        }

        public IEntitySerialization GenerateEntitySerializer()
        {
            return new EntitySerialization(this);
        }
    }
}