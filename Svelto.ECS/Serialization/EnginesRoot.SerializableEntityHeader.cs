using Svelto.DataStructures;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        struct SerializableEntityHeader
        {
            public readonly uint descriptorHash;
            public readonly byte entityStructsCount;

            const uint SIZE = 4 + 4 + 4 + 1;

            internal SerializableEntityHeader(uint descriptorHash_, EGID egid_, byte entityStructsCount_)
            {
                entityStructsCount = entityStructsCount_;
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
                     | serializationData.data[serializationData.dataPos++] << 16
                     | serializationData.data[serializationData.dataPos++] << 24);

                entityStructsCount = serializationData.data[serializationData.dataPos++];

                egid = new EGID(entityID, new ExclusiveGroup.ExclusiveGroupStruct(groupID));
            }

            internal void Copy(ISerializationData serializationData)
            {
                serializationData.data.ExpandBy(SIZE);

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
                uint groupID = egid.groupID;
                serializationData.data[serializationData.dataPos++] = (byte) (groupID & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((groupID >> 8) & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((groupID >> 16) & 0xff);
                serializationData.data[serializationData.dataPos++] = (byte) ((groupID >> 24) & 0xff);

                serializationData.data[serializationData.dataPos++] = entityStructsCount;
            }

            internal readonly EGID egid; //this can't be used safely!
        }
    }
}
