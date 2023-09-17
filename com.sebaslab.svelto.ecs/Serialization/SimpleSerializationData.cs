using Svelto.DataStructures;

namespace Svelto.ECS.Serialization
{
    public class SimpleSerializationData : ISerializationData
    {
        public uint dataPos { get; set; }
        public FasterList<byte> data { get; set; }

        public SimpleSerializationData(FasterList<byte> d)
        {
            data = d;
        }

        public void ResetWithNewData(FasterList<byte> newData)
        {
            dataPos = 0;

            data = newData;
        }

        public void ReuseAsNew()
        {
            dataPos = 0;

            data.ResetToReuse();
        }
        
        public void Reset()
        {
            dataPos = 0;
        }

        public void BeginNextEntityComponent()
        {}
    }
}