using Svelto.ECS.Serialization;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        readonly struct SerializableEntityHeader
        {
            public readonly uint descriptorHash;
            public readonly byte entityComponentsCount;

            const uint SIZE = 4 + 4 + 4 + 1;

            internal SerializableEntityHeader(uint descriptorHash_, EGID egid_, byte entityComponentsCount_)
            {
                entityComponentsCount = entityComponentsCount_;
                descriptorHash = descriptorHash_;
                egid = egid_;
            }

            internal SerializableEntityHeader(ISerializationData serializationData)
            {
                descriptorHash = (uint)
                    (serializationData.data[serializationData.dataPos++]
                     | serializationData.data[serializationData.dataPos++] << 8
                     | serializationData.data[serializationData.dataPos++] << 16
                     | serializationData.data[serializationData.dataPos++] << 24);

                uint entityID = (uint)
                    (serializationData.data[serializationData.dataPos++]
                     | serializationData.data[serializationData.dataPos++] << 8
                     | serializationData.data[serializationData.dataPos++] << 16
                     | serializationData.data[serializationData.dataPos++] << 24);

                uint groupID = (uint)
                    (serializationData.data[serializationData.dataPos++]    
                     | serializationData.data[serializationData.dataPos++] << 8
                     | serializationData.data[serializationData.dataPos++] << 16);
                var byteMask = serializationData.data[serializationData.dataPos++];

                entityComponentsCount = serializationData.data[serializationData.dataPos++];

                egid = new EGID(entityID, new ExclusiveGroupStruct(groupID, byteMask));
            }

            internal void Copy(ISerializationData serializationData)
            {
                serializationData.data.IncrementCountBy(SIZE);

                // Splitting the descriptorHash_ (uint, 32 bit) into four bytes.
                serializationData.data[serializationData.dataPos++] = (byte) (descriptorHash & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((descriptorHash >> 8) & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((descriptorHash >> 16) & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((descriptorHash >> 24) & 0xff);

                // Splitting the entityID (uint, 32 bit) into four bytes.
                uint entityID = egid.entityID;
                serializationData.data[serializationData.dataPos++] = (byte) (entityID & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((entityID >> 8) & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((entityID >> 16) & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((entityID >> 24) & 0xff);

                // Splitting the groupID (uint, 32 bit) into four bytes.
                var groupID = egid.groupID.ToIDAndBitmask();
                serializationData.data[serializationData.dataPos++] = (byte) (groupID & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((groupID >> 8) & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((groupID >> 16) & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((groupID >> 24) & 0xff);

                serializationData.data[serializationData.dataPos++] = entityComponentsCount;
            }

            internal readonly EGID egid; //this can't be used safely!
        }
    }
}
