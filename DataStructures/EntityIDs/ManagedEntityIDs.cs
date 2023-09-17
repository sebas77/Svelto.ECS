using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public struct ManagedEntityIDs: IEntityIDs
    {
        internal ManagedEntityIDs(MB<SveltoDictionaryNode<uint>> managed)
        {
            _managed = managed;
        }
        
        internal void Update(MB<SveltoDictionaryNode<uint>> managed)
        {
            _managed = managed;
        }

        public uint this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _managed[index].key;
        }

        public uint this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _managed[index].key;
        }

        MBInternal<SveltoDictionaryNode<uint>> _managed;
    }
}