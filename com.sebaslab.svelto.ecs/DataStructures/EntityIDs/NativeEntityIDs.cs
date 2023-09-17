using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public struct NativeEntityIDs: IEntityIDs
    {
        internal NativeEntityIDs(NB<SveltoDictionaryNode<uint>> native)
        {
            _native = native;
        }

        public void Update(in NB<SveltoDictionaryNode<uint>> unsafeKeys)
        {
            _native = unsafeKeys;
        }

        public uint this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _native[index].key;
        }

        public uint this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _native[index].key;
        }

        NBInternal<SveltoDictionaryNode<uint>> _native;
    }
}