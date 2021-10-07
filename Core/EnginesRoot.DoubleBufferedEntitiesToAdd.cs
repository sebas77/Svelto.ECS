using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        internal class DoubleBufferedEntitiesToAdd
        {
            const int MAX_NUMBER_OF_ITEMS_PER_FRAME_BEFORE_TO_CLEAR = 100;

            public DoubleBufferedEntitiesToAdd()
            {
                _currentEntitiesCreatedPerGroup = _entitiesCreatedPerGroupA;
                _otherEntitiesCreatedPerGroup   = _entitiesCreatedPerGroupB;

                current = _entityComponentsToAddBufferA;
                other   = _entityComponentsToAddBufferB;
            }

            public void ClearOther()
            {
                //do not clear the groups created so far, they will be reused, unless they are too many!
                var otherCount = other.count;
                if (otherCount > MAX_NUMBER_OF_ITEMS_PER_FRAME_BEFORE_TO_CLEAR)
                {
                    var otherValuesArray = other.unsafeValues;
                    for (var i = 0; i < otherCount; ++i)
                    {
                        var safeDictionariesCount = otherValuesArray[i].count;
                        var safeDictionaries      = otherValuesArray[i].unsafeValues;
                        {
                            for (var j = 0; j < safeDictionariesCount; ++j)
                                //clear the dictionary of entities create do far (it won't allocate though)
                                safeDictionaries[j].Dispose();
                        }
                    }

                    //reset the number of entities created so far
                    _otherEntitiesCreatedPerGroup.FastClear();
                    other.FastClear();
                    return;
                }

                {
                    var otherValuesArray = other.unsafeValues;
                    for (var i = 0; i < otherCount; ++i)
                    {
                        var safeDictionariesCount = otherValuesArray[i].count;
                        var safeDictionaries      = otherValuesArray[i].unsafeValues;
                        //do not remove the dictionaries of entities per type created so far, they will be reused
                        if (safeDictionariesCount <= MAX_NUMBER_OF_ITEMS_PER_FRAME_BEFORE_TO_CLEAR)
                        {
                            for (var j = 0; j < safeDictionariesCount; ++j)
                                //clear the dictionary of entities create do far (it won't allocate though)
                                safeDictionaries[j].FastClear();
                        }
                        else
                        {
                            for (var j = 0; j < safeDictionariesCount; ++j)
                                //clear the dictionary of entities create do far (it won't allocate though)
                                safeDictionaries[j].Dispose();

                            otherValuesArray[i].FastClear();
                        }
                    }

                    //reset the number of entities created so far
                    _otherEntitiesCreatedPerGroup.FastClear();
                }
            }

            public void Dispose()
            {
                {
                    var otherValuesArray = other.unsafeValues;
                    for (var i = 0; i < other.count; ++i)
                    {
                        var safeDictionariesCount = otherValuesArray[i].count;
                        var safeDictionaries      = otherValuesArray[i].unsafeValues;
                        //do not remove the dictionaries of entities per type created so far, they will be reused
                        for (var j = 0; j < safeDictionariesCount; ++j)
                            //clear the dictionary of entities create do far (it won't allocate though)
                            safeDictionaries[j].Dispose();
                    }
                }
                {
                    var currentValuesArray = current.unsafeValues;
                    for (var i = 0; i < current.count; ++i)
                    {
                        var safeDictionariesCount = currentValuesArray[i].count;
                        var safeDictionaries      = currentValuesArray[i].unsafeValues;
                        //do not remove the dictionaries of entities per type created so far, they will be reused
                        for (var j = 0; j < safeDictionariesCount; ++j)
                            //clear the dictionary of entities create do far (it won't allocate though)
                            safeDictionaries[j].Dispose();
                    }
                }
            }

            internal bool AnyEntityCreated()
            {
                return _currentEntitiesCreatedPerGroup.count > 0;
            }

            internal bool AnyOtherEntityCreated()
            {
                return _otherEntitiesCreatedPerGroup.count > 0;
            }

            internal void IncrementEntityCount(ExclusiveGroupStruct groupID)
            {
                _currentEntitiesCreatedPerGroup.GetOrCreate(groupID)++;
            }

            internal void Preallocate
                (ExclusiveGroupStruct groupID, uint numberOfEntities, IComponentBuilder[] entityComponentsToBuild)
            {
                void PreallocateDictionaries
                    (FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>> dic)
                {
                    var group = dic.GetOrCreate(groupID, () => new FasterDictionary<RefWrapperType,
                                                                      ITypeSafeDictionary>());

                    foreach (var componentBuilder in entityComponentsToBuild)
                    {
                        var entityComponentType = componentBuilder.GetEntityComponentType();
                        var safeDictionary = group.GetOrCreate(new RefWrapperType(entityComponentType)
                                                             , () => componentBuilder
                                                                  .CreateDictionary(numberOfEntities));
                        componentBuilder.Preallocate(safeDictionary, numberOfEntities);
                    }
                }

                PreallocateDictionaries(current);
                PreallocateDictionaries(other);

                _currentEntitiesCreatedPerGroup.GetOrCreate(groupID);
                _otherEntitiesCreatedPerGroup.GetOrCreate(groupID);
            }

            internal void Swap()
            {
                Swap(ref current, ref other);
                Swap(ref _currentEntitiesCreatedPerGroup, ref _otherEntitiesCreatedPerGroup);
            }

            void Swap<T>(ref T item1, ref T item2)
            {
                var toSwap = item2;
                item2 = item1;
                item1 = toSwap;
            }

            //Before I tried for the third time to use a SparseSet instead of FasterDictionary, remember that
            //while group indices are sequential, they may not be used in a sequential order. Sparseset needs
            //entities to be created sequentially (the index cannot be managed externally)
            internal FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>> current;
            internal FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>> other;

            readonly FasterDictionary<ExclusiveGroupStruct, uint> _entitiesCreatedPerGroupA =
                new FasterDictionary<ExclusiveGroupStruct, uint>();

            readonly FasterDictionary<ExclusiveGroupStruct, uint> _entitiesCreatedPerGroupB =
                new FasterDictionary<ExclusiveGroupStruct, uint>();

            readonly FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>> _entityComponentsToAddBufferA =
                    new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>();

            readonly FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>> _entityComponentsToAddBufferB =
                    new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>();

            /// <summary>
            ///     To avoid extra allocation, I don't clear the groups, so I need an extra data structure
            ///     to keep count of the number of entities built this frame. At the moment the actual number
            ///     of entities built is not used
            /// </summary>
            FasterDictionary<ExclusiveGroupStruct, uint> _currentEntitiesCreatedPerGroup;
            FasterDictionary<ExclusiveGroupStruct, uint> _otherEntitiesCreatedPerGroup;
        }
    }
}