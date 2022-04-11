using System.Runtime.CompilerServices;
using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct EntityFilterIndices
    {
        public EntityFilterIndices(NB<uint> indices, uint count)
        {
            _indices = indices;
            _count   = count;
            _index   = 0;
        }

        public uint count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Get(uint index) => _indices[index];

        public uint this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _indices[index];
        }

        public uint this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _indices[index];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Next()
        {
            return _indices[Interlocked.Increment(ref _index) - 1];
        }

        readonly NB<uint> _indices;
        readonly uint     _count;
        int               _index;
    }
}