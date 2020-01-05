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
                var otherCount = other.Count;
                if (otherCount > MaximumNumberOfItemsPerFrameBeforeToClear)
                {
                    otherEntitiesCreatedPerGroup.FastClear();
                    other.FastClear();
                    return;
                }
                var otherValuesArray = other.valuesArray;
                for (int i = 0; i < otherCount; ++i)
                {
                    var safeDictionariesCount = otherValuesArray[i].Count;
                    var safeDictionaries = otherValuesArray[i].valuesArray;
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

            internal FasterDictionary<uint, uint> currentEntitiesCreatedPerGroup;
            internal FasterDictionary<uint, uint> otherEntitiesCreatedPerGroup;

            internal FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> current;
            internal FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> other;

            readonly FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>
                _entityViewsToAddBufferA =
                    new FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>();

            readonly FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>
                _entityViewsToAddBufferB =
                    new FasterDictionary<uint, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>();

            readonly FasterDictionary<uint, uint> _entitiesCreatedPerGroupA = new FasterDictionary<uint, uint>();
            readonly FasterDictionary<uint, uint> _entitiesCreatedPerGroupB = new FasterDictionary<uint, uint>();

            public DoubleBufferedEntitiesToAdd()
            {
                currentEntitiesCreatedPerGroup = _entitiesCreatedPerGroupA;
                otherEntitiesCreatedPerGroup = _entitiesCreatedPerGroupB;

                current = _entityViewsToAddBufferA;
                other = _entityViewsToAddBufferB;
            }
        }
    }
}