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
                    FasterDictionary<RefWrapperType, ITypeSafeDictionary>[] otherValuesArray = other.unsafeValues;
                    for (int i = 0; i < otherCount; ++i)
                    {
                        var                   safeDictionariesCount = otherValuesArray[i].count;
                        ITypeSafeDictionary[] safeDictionaries      = otherValuesArray[i].unsafeValues;
                        {
                            for (int j = 0; j < safeDictionariesCount; ++j)
                            {
                                //clear the dictionary of entities create do far (it won't allocate though)
                                safeDictionaries[j].Dispose();
                            }
                        }
                    }
                    
                    //reset the number of entities created so far
                    otherEntitiesCreatedPerGroup.FastClear();
                    other.FastClear();
                    return;
                }

                {
                    FasterDictionary<RefWrapperType, ITypeSafeDictionary>[] otherValuesArray = other.unsafeValues;
                    for (int i = 0; i < otherCount; ++i)
                    {
                        var                   safeDictionariesCount = otherValuesArray[i].count;
                        ITypeSafeDictionary[] safeDictionaries      = otherValuesArray[i].unsafeValues;
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
                            for (int j = 0; j < safeDictionariesCount; ++j)
                            {
                                //clear the dictionary of entities create do far (it won't allocate though)
                                safeDictionaries[j].Dispose();
                            }

                            otherValuesArray[i].FastClear();
                        }
                    }

                    //reset the number of entities created so far
                    otherEntitiesCreatedPerGroup.FastClear();
                }
            }
            
            //Before I tried for the third time to use a SparseSet instead of FasterDictionary, remember that
            //while group indices are sequential, they may not be used in a sequential order. Sparseset needs
            //entities to be created sequentially (the index cannot be managed externally)
            internal FasterDictionary<uint, FasterDictionary<RefWrapperType, ITypeSafeDictionary>> current;
            internal FasterDictionary<uint, FasterDictionary<RefWrapperType, ITypeSafeDictionary>> other;

            /// <summary>
            /// To avoid extra allocation, I don't clear the groups, so I need an extra data structure
            /// to keep count of the number of entities built this frame. At the moment the actual number
            /// of entities built is not used
            /// </summary>
            FasterDictionary<ExclusiveGroupStruct, uint> currentEntitiesCreatedPerGroup;
            FasterDictionary<ExclusiveGroupStruct, uint> otherEntitiesCreatedPerGroup;

            readonly FasterDictionary<uint, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>
                _entityComponentsToAddBufferA =
                    new FasterDictionary<uint, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>();

            readonly FasterDictionary<uint, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>
                _entityComponentsToAddBufferB =
                    new FasterDictionary<uint, FasterDictionary<RefWrapperType, ITypeSafeDictionary>>();

            readonly FasterDictionary<ExclusiveGroupStruct, uint> _entitiesCreatedPerGroupA = new FasterDictionary<ExclusiveGroupStruct, uint>();
            readonly FasterDictionary<ExclusiveGroupStruct, uint> _entitiesCreatedPerGroupB = new FasterDictionary<ExclusiveGroupStruct, uint>();

            public DoubleBufferedEntitiesToAdd()
            {
                currentEntitiesCreatedPerGroup = _entitiesCreatedPerGroupA;
                otherEntitiesCreatedPerGroup = _entitiesCreatedPerGroupB;

                current = _entityComponentsToAddBufferA;
                other = _entityComponentsToAddBufferB;
            }

            public void Dispose()
            {
                {
                    var otherValuesArray = other.unsafeValues;
                    for (int i = 0; i < other.count; ++i)
                    {
                        var safeDictionariesCount = otherValuesArray[i].count;
                        var safeDictionaries      = otherValuesArray[i].unsafeValues;
                        //do not remove the dictionaries of entities per type created so far, they will be reused
                        for (int j = 0; j < safeDictionariesCount; ++j)
                        {
                            //clear the dictionary of entities create do far (it won't allocate though)
                            safeDictionaries[j].Dispose();
                        }
                    }
                }
                {
                    var currentValuesArray = current.unsafeValues;
                    for (int i = 0; i < current.count; ++i)
                    {
                        var safeDictionariesCount = currentValuesArray[i].count;
                        var safeDictionaries      = currentValuesArray[i].unsafeValues;
                        //do not remove the dictionaries of entities per type created so far, they will be reused
                        for (int j = 0; j < safeDictionariesCount; ++j)
                        {
                            //clear the dictionary of entities create do far (it won't allocate though)
                            safeDictionaries[j].Dispose();
                        }
                    }
                }
            }

            internal void IncrementEntityCount(ExclusiveGroupStruct groupID)
            {
                currentEntitiesCreatedPerGroup.GetOrCreate(groupID)++;
            }

            internal bool AnyEntityCreated()
            {
                return currentEntitiesCreatedPerGroup.count > 0;
            }

            internal bool AnyOtherEntityCreated()
            {
                return otherEntitiesCreatedPerGroup.count > 0;
            }

            internal void Preallocate
                (ExclusiveGroupStruct groupID, uint numberOfEntities, IComponentBuilder[] entityComponentsToBuild)
            {
                void PreallocateDictionaries(FasterDictionary<uint, FasterDictionary<RefWrapperType, ITypeSafeDictionary>> fasterDictionary1)
                {
                    FasterDictionary<RefWrapperType, ITypeSafeDictionary> group =
                        fasterDictionary1.GetOrCreate((uint) groupID, () => new FasterDictionary<RefWrapperType, ITypeSafeDictionary>());

                    foreach (var componentBuilder in entityComponentsToBuild)
                    {
                        var entityComponentType = componentBuilder.GetEntityComponentType();
                        var safeDictionary = @group.GetOrCreate(new RefWrapperType(entityComponentType)
                                                             , () => componentBuilder.CreateDictionary(numberOfEntities));
                        componentBuilder.Preallocate(safeDictionary, numberOfEntities);
                    }
                }

                PreallocateDictionaries(current);
                PreallocateDictionaries(other);

                currentEntitiesCreatedPerGroup.GetOrCreate(groupID);
                otherEntitiesCreatedPerGroup.GetOrCreate(groupID);
            }
        }
    }
}