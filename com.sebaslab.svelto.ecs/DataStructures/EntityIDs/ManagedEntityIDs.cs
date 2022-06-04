using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public struct ManagedEntityIDs: IEntityIDs
    {
        public ManagedEntityIDs(MB<SveltoDictionaryNode<uint>> managed)
        {
            _managed = managed;
        }
        
        public void Update(MB<SveltoDictionaryNode<uint>> managed)
        {
            _managed = managed;
        }

        public uint this[uint index] => _managed[index].key;
        public uint this[int index] => _managed[index].key;

        MB<SveltoDictionaryNode<uint>> _managed;
    }
}