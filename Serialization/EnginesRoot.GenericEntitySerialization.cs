using System;
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
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);
                var entityComponentsToSerialise = entityDescriptor.componentsToSerialize;

                var header =
                    new SerializableEntityHeader(descriptorHash, egid, (byte) entityComponentsToSerialise.Length);
                header.Copy(serializationData);

                for (int index = 0; index < entityComponentsToSerialise.Length; index++)
                {
                    var entityBuilder = entityComponentsToSerialise[index];

                    serializationData.BeginNextEntityComponent();
                    SerializeEntityComponent(egid, entityBuilder, serializationData, serializationType);
                }
            }

            public EntityInitializer DeserializeNewEntity
                (EGID egid, ISerializationData serializationData, int serializationType)
            {
                //todo: SerializableEntityHeader may be needed to be customizable
                var serializableEntityHeader = new SerializableEntityHeader(serializationData);

                uint descriptorHash = serializableEntityHeader.descriptorHash;
                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot._serializationDescriptorMap;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);
                IDeserializationFactory factory = serializationDescriptorMap.GetSerializationFactory(descriptorHash);

                return factory.BuildDeserializedEntity(egid, serializationData, entityDescriptor, serializationType
                                                     , this, this._enginesRoot.GenerateEntityFactory()
                                                     , _enginesRoot._isDeserializationOnly);
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

            public void DeserializeEntityComponents
            (ISerializationData serializationData, ISerializableEntityDescriptor entityDescriptor
           , ref EntityInitializer initializer, int serializationType)
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
            public T DeserializeEntityComponent<T>
            (ISerializationData serializationData, ISerializableEntityDescriptor entityDescriptor
           , int serializationType) where T : unmanaged, IEntityComponent
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
                        serializationData.dataPos += serializableEntityBuilder.Size(serializationType);
                }

                serializationData.dataPos = readPos;
                return entityComponent;
            }

            public void DeserializeEntityToSwap(EGID localEgid, EGID toEgid)
            {
                EntitiesDB entitiesDb = _enginesRoot._entitiesDB;
                ref var serializableEntityComponent =
                    ref entitiesDb.QueryEntity<SerializableEntityComponent>(localEgid);

                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot._serializationDescriptorMap;
                uint descriptorHash = serializableEntityComponent.descriptorHash;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);

                var entitySubmitOperation =
                    new EntitySubmitOperation(EntitySubmitOperationType.Swap, localEgid, toEgid
                                            , entityDescriptor.componentsToBuild);

                _enginesRoot.CheckRemoveEntityID(localEgid, entityDescriptor.realType);
                _enginesRoot.CheckAddEntityID(toEgid, entityDescriptor.realType);

                _enginesRoot.QueueEntitySubmitOperation(entitySubmitOperation);
            }

            public void DeserializeEntityToDelete(EGID egid)
            {
                EntitiesDB entitiesDB                  = _enginesRoot._entitiesDB;
                ref var    serializableEntityComponent = ref entitiesDB.QueryEntity<SerializableEntityComponent>(egid);
                uint       descriptorHash              = serializableEntityComponent.descriptorHash;

                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot._serializationDescriptorMap;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);

                _enginesRoot.CheckRemoveEntityID(egid, entityDescriptor.realType);

                var entitySubmitOperation =
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, egid, egid
                                            , entityDescriptor.componentsToBuild);

                _enginesRoot.QueueEntitySubmitOperation(entitySubmitOperation);
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

            internal EntitySerialization(EnginesRoot enginesRoot) { _enginesRoot = enginesRoot; }

            void SerializeEntityComponent
            (EGID entityGID, ISerializableComponentBuilder componentBuilder, ISerializationData serializationData
           , int serializationType)
            {
                ExclusiveGroupStruct groupId    = entityGID.groupID;
                Type                 entityType = componentBuilder.GetEntityComponentType();
                if (!_enginesRoot._entitiesDB.UnsafeQueryEntityDictionary(groupId, entityType, out var safeDictionary))
                {
                    throw new Exception("Entity Serialization failed");
                }

                componentBuilder.Serialize(entityGID.entityID, safeDictionary, serializationData, serializationType);
            }

            void DeserializeEntityInternal
            (ISerializationData serializationData, EGID egid, SerializableEntityHeader serializableEntityHeader
           , int serializationType)
            {
                SerializationDescriptorMap descriptorMap = _enginesRoot._serializationDescriptorMap;
                var entityDescriptor = descriptorMap.GetDescriptorFromHash(serializableEntityHeader.descriptorHash);

                if (_enginesRoot._groupEntityComponentsDB.TryGetValue(egid.groupID, out var entitiesInGroupPerType)
                 == false)
                    throw new Exception("Entity Serialization failed");

                foreach (var serializableEntityBuilder in entityDescriptor.componentsToSerialize)
                {
                    entitiesInGroupPerType.TryGetValue(
                        new RefWrapperType(serializableEntityBuilder.GetEntityComponentType()), out var safeDictionary);

                    serializationData.BeginNextEntityComponent();
                    serializableEntityBuilder.Deserialize(egid.entityID, safeDictionary, serializationData
                                                        , serializationType);
                }
            }

            readonly EnginesRoot _enginesRoot;
        }

        public IEntitySerialization GenerateEntitySerializer() { return new EntitySerialization(this); }

        readonly bool _isDeserializationOnly;
    }
}