using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        internal class DoubleBufferedEntitiesToAdd
        {
            const int MaximumNumberOfItemsPerFrameBeforeToClear = 100;

            internal void Swap()
            {
                Swap(ref current, ref other);
                Swap(ref currentEntitiesCreatedPerGroup, ref otherEntitiesCreatedPerGroup);
            }

            void Swap<T>(ref T item1, ref T item2)
            {
                var toSwap = item2;
                item2 = item1;
                item1 = toSwap;
            }

            public void ClearOther()
            {
                //do not clear the groups created so far, they will be reused, unless they are too many!
                var otherCount = other.count;
                if (otherCount > MaximumNumberOfItemsPerFrameBeforeToClear)
                {
                    otherEntitiesCreatedPerGroup.FastClear();
                    other.FastClear();
                    return;
                }
                var otherValuesArray = other.unsafeValues;
                for (int i = 0; i < otherCount; ++i)
                {
                    var safeDictionariesCount = otherValuesArray[i].count;
                    var safeDictionaries = otherValuesArray[i].unsafeValues;
                    //do not remove the dictionaries of entities per type created so far, they will be reused
                    if (safeDictionariesCount <= MaximumNumberOfItemsPerFrameBeforeToClear)
                    {
                        for (int j = 0; j < safeDictionariesCount; ++j)
                        {
                            //clear the dictionary of entities create do far (it won't allocate though)
                            safeDictionaries[j].FastClear();
                        }
                    }
                    else
                    {
                        otherValuesArray[i].FastClear();
                    }
                }

                otherEntitiesCreatedPerGroup.FastClear();
            }

            /// <summary>
            /// To avoid extra allocation, I don't clear the dictionaries, so I need an extra data structure
            /// to keep count of the number of entities submitted this frame
            /// </summary>
            internal FasterDictionary<uint, uint> currentEntitiesCreatedPerGroup;
            internal FasterDictionary<uint, uint> otherEntitiesCreatedPerGroup;

            //Before I tried for the third time to use a SparseSet instead of FasterDictionary, remember that
            //while group indices are sequential, they may not be used in a sequential order. Sparaset needs
            //entities to be created sequentially (the index cannot be managed externally)
            internal FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> current;
            internal FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> other;

            readonly FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>
                _entityComponentsToAddBufferA =
                    new FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>();

            readonly FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>
                _entityComponentsToAddBufferB =
                    new FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>();

            readonly FasterDictionary<uint, uint> _entitiesCreatedPerGroupA = new FasterDictionary<uint, uint>();
            readonly FasterDictionary<uint, uint> _entitiesCreatedPerGroupB = new FasterDictionary<uint, uint>();

            public DoubleBufferedEntitiesToAdd()
            {
                currentEntitiesCreatedPerGroup = _entitiesCreatedPerGroupA;
                otherEntitiesCreatedPerGroup = _entitiesCreatedPerGroupB;

                current = _entityComponentsToAddBufferA;
                other = _entityComponentsToAddBufferB;
            }
        }
    }
}