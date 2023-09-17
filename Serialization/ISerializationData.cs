using Svelto.DataStructures;

namespace Svelto.ECS.Serialization
{
    public interface ISerializationData
    {
        uint dataPos { get; set; }
        FasterList<byte> data { get; }

        void ReuseAsNew();
        void Reset();
        void BeginNextEntityComponent();
    }
}