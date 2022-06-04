using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public struct NativeEntityIDs: IEntityIDs
    {
        public NativeEntityIDs(NB<SveltoDictionaryNode<uint>> native)
        {
            _native = native;
        }
        
        public void Update(in NB<SveltoDictionaryNode<uint>> unsafeKeys)
        {
            _native = unsafeKeys;
        }

        public uint this[uint index] => _native[index].key;
        public uint this[int index] => _native[index].key;

        NB<SveltoDictionaryNode<uint>> _native;
    }
}