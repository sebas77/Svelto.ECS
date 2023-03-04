#if SVELTO_LEGACY_FILTERS
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public readonly struct LegacyFilteredIndices
    {
        public LegacyFilteredIndices(NativeDynamicArrayCast<uint> denseListOfIndicesToEntityComponentArray)
        {
            _denseListOfIndicesToEntityComponentArray = denseListOfIndicesToEntityComponentArray;
            _count                                    = _denseListOfIndicesToEntityComponentArray.count;
        }

        public int count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _count; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Get(uint index) => _denseListOfIndicesToEntityComponentArray[index];

        public uint this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _denseListOfIndicesToEntityComponentArray[index];
        }

        public uint this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _denseListOfIndicesToEntityComponentArray[index];
        }

        readonly NativeDynamicArrayCast<uint> _denseListOfIndicesToEntityComponentArray;
        readonly int                          _count;
    }
}
#endif