using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        internal class DoubleBufferedEntitiesToAdd
        {
            //while caching is good to avoid over creating dictionaries that may be reused, the side effect
            //is that I have to iterate every time up to 100 dictionaries during the flushing of the build entities
            //even if there are 0 entities inside.
            const int MAX_NUMBER_OF_GROUPS_TO_CACHE          = 100;
            const int MAX_NUMBER_OF_TYPES_PER_GROUP_TO_CACHE = 100;

            public DoubleBufferedEntitiesToAdd()
            {
                var entitiesCreatedPerGroupA = new FasterDictionary<ExclusiveGroupStruct, uint>();
                var entitiesCreatedPerGroupB = new FasterDictionary<ExclusiveGroupStruct, uint>();
                var entityComponentsToAddBufferA =
                    new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>();
                var entityComponentsToAddBufferB =
                    new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>();

                _currentNumberEntitiesCreatedPerGroup = entitiesCreatedPerGroupA;
                _lastNumberEntitiesCreatedPerGroup   = entitiesCreatedPerGroupB;

                currentComponentsToAddPerGroup = entityComponentsToAddBufferA;
                lastComponentsToAddPerGroup   = entityComponentsToAddBufferB;
            }

            public void ClearLastAddOperations()
            {
                var numberOfGroupsAddedSoFar     = lastComponentsToAddPerGroup.count;
                var componentDictionariesPerType = lastComponentsToAddPerGroup.unsafeValues;
                
                //TODO: rewrite the caching logic with the new RecycleOrAdd dictionary functionality
                //I still do not want to cache too many groups
                
                //If we didn't create too many groups, we keep them alive, so we avoid the cost of creating new dictionaries
                //during future submissions, otherwise we clean up everything
                if (numberOfGroupsAddedSoFar > MAX_NUMBER_OF_GROUPS_TO_CACHE)
                {
                    for (var i = 0; i < numberOfGroupsAddedSoFar; ++i)
                    {
                        var componentTypesCount      = componentDictionariesPerType[i].count;
                        var componentTypesDictionary = componentDictionariesPerType[i].unsafeValues;
                        {
                            for (var j = 0; j < componentTypesCount; ++j)
                                //dictionaries of components may be native so they need to be disposed
                                //before the references are GCed
                                componentTypesDictionary[j].Dispose();
                        }
                    }
                
                    //reset the number of entities created so far
                    _lastNumberEntitiesCreatedPerGroup.Clear();
                    lastComponentsToAddPerGroup.Clear();
                
                    return;
                }
                
                for (var i = 0; i < numberOfGroupsAddedSoFar; ++i)
                {
                    var                   componentTypesCount      = componentDictionariesPerType[i].count;
                    ITypeSafeDictionary[] componentTypesDictionary = componentDictionariesPerType[i].unsafeValues;
                    for (var j = 0; j < componentTypesCount; ++j)
                        //clear the dictionary of entities created so far (it won't allocate though)
                        componentTypesDictionary[j].Clear();
                
                    //if we didn't create too many component for this group, I reuse the component arrays
                    if (componentTypesCount <= MAX_NUMBER_OF_TYPES_PER_GROUP_TO_CACHE)
                    {
                        for (var j = 0; j < componentTypesCount; ++j)
                            componentTypesDictionary[j].Clear();
                    }
                    else
                    {
                        //here I have to dispose, because I am actually clearing the reference of the dictionary
                        //with the next line.
                        for (var j = 0; j < componentTypesCount; ++j)
                            componentTypesDictionary[j].Dispose();
                
                        componentDictionariesPerType[i].Clear();
                    }
                }

                //reset the number of entities created so far
                _lastNumberEntitiesCreatedPerGroup.Clear();

          //      _totalEntitiesToAdd = 0;
            }

            public void Dispose()
            {
                {
                    var otherValuesArray = lastComponentsToAddPerGroup.unsafeValues;
                    for (var i = 0; i < lastComponentsToAddPerGroup.count; ++i)
                    {
                        int                   safeDictionariesCount = otherValuesArray[i].count;
                        ITypeSafeDictionary[] safeDictionaries      = otherValuesArray[i].unsafeValues;
                        //do not remove the dictionaries of entities per type created so far, they will be reused
                        for (var j = 0; j < safeDictionariesCount; ++j)
                            //clear the dictionary of entities create do far (it won't allocate though)
                            safeDictionaries[j].Dispose();
                    }
                }
                {
                    var currentValuesArray = currentComponentsToAddPerGroup.unsafeValues;
                    for (var i = 0; i < currentComponentsToAddPerGroup.count; ++i)
                    {
                        int                   safeDictionariesCount = currentValuesArray[i].count;
                        ITypeSafeDictionary[] safeDictionaries      = currentValuesArray[i].unsafeValues;
                        //do not remove the dictionaries of entities per type created so far, they will be reused
                        for (var j = 0; j < safeDictionariesCount; ++j)
                            //clear the dictionary of entities create do far (it won't allocate though)
                            safeDictionaries[j].Dispose();
                    }
                }

                _currentNumberEntitiesCreatedPerGroup = null;
                _lastNumberEntitiesCreatedPerGroup   = null;
                lastComponentsToAddPerGroup          = null;
                currentComponentsToAddPerGroup        = null;
            }

            internal bool AnyEntityCreated()
            {
                return _currentNumberEntitiesCreatedPerGroup.count > 0;
            }

            internal bool AnyPreviousEntityCreated()
            {
                return _lastNumberEntitiesCreatedPerGroup.count > 0;
            }

            internal void IncrementEntityCount(ExclusiveGroupStruct groupID)
            {
                _currentNumberEntitiesCreatedPerGroup.GetOrAdd(groupID)++;
             //   _totalEntitiesToAdd++;
            }

            // public uint NumberOfEntitiesToAdd()
            // {
            //     return _totalEntitiesToAdd;
            // }

            internal void Preallocate
                (ExclusiveGroupStruct groupID, uint numberOfEntities, IComponentBuilder[] entityComponentsToBuild)
            {
                void PreallocateDictionaries
                    (FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>> dic)
                {
                    //get the set of entities in the group ID
                    var group = dic.GetOrAdd(
                        groupID, () => new FasterDictionary<ComponentID, ITypeSafeDictionary>());

                    //for each component of the entities in the group
                    foreach (var componentBuilder in entityComponentsToBuild)
                    {
                        //get the dictionary of entities for the component type
                        var components = group.GetOrAdd(componentBuilder.getComponentID, () => componentBuilder.CreateDictionary(numberOfEntities));
                        
                        componentBuilder.Preallocate(components, numberOfEntities);
                    }
                }

                PreallocateDictionaries(currentComponentsToAddPerGroup);
                PreallocateDictionaries(lastComponentsToAddPerGroup);

                _currentNumberEntitiesCreatedPerGroup.GetOrAdd(groupID);
                _lastNumberEntitiesCreatedPerGroup.GetOrAdd(groupID);
            }

            internal void Swap()
            {
                Swap(ref currentComponentsToAddPerGroup, ref lastComponentsToAddPerGroup);
                Swap(ref _currentNumberEntitiesCreatedPerGroup, ref _lastNumberEntitiesCreatedPerGroup);
            }

            static void Swap<T>(ref T item1, ref T item2)
            {
                (item2, item1) = (item1, item2);
            }

            public OtherComponentsToAddPerGroupEnumerator GetEnumerator()
            {
                return new OtherComponentsToAddPerGroupEnumerator(lastComponentsToAddPerGroup
                                                                , _lastNumberEntitiesCreatedPerGroup);
            }

            //Before I tried for the third time to use a SparseSet instead of FasterDictionary, remember that
            //while group indices are sequential, they may not be used in a sequential order. Sparseset needs
            //entities to be created sequentially (the index cannot be managed externally)
            internal FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>
                currentComponentsToAddPerGroup;

            FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>
                lastComponentsToAddPerGroup;

            /// <summary>
            ///     To avoid extra allocation, I don't clear the groups, so I need an extra data structure
            ///     to keep count of the number of entities built this frame. At the moment the actual number
            ///     of entities built is not used
            /// </summary>
            FasterDictionary<ExclusiveGroupStruct, uint> _currentNumberEntitiesCreatedPerGroup;
            FasterDictionary<ExclusiveGroupStruct, uint> _lastNumberEntitiesCreatedPerGroup;

            //uint _totalEntitiesToAdd;
        }
    }

    struct OtherComponentsToAddPerGroupEnumerator
    {
        public OtherComponentsToAddPerGroupEnumerator
        (FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>
             lastComponentsToAddPerGroup
       , FasterDictionary<ExclusiveGroupStruct, uint> otherNumberEntitiesCreatedPerGroup)
        {
            _lastComponentsToAddPerGroup       = lastComponentsToAddPerGroup;
            _lastNumberEntitiesCreatedPerGroup = otherNumberEntitiesCreatedPerGroup.GetEnumerator();
            Current                             = default;
        }

        public bool MoveNext()
        {
            while (_lastNumberEntitiesCreatedPerGroup.MoveNext())
            {
                var current = _lastNumberEntitiesCreatedPerGroup.Current;

                if (current.value > 0) //there are entities in this group
                {
                    var value = _lastComponentsToAddPerGroup[current.key];
                    Current = new GroupInfo()
                    {
                        group      = current.key
                      , components = value
                    };

                    return true;
                }
            }

            return false;
        }

        public GroupInfo Current { get; private set; }

        //cannot be read only as they will be modified by MoveNext
        readonly FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, ITypeSafeDictionary>>
            _lastComponentsToAddPerGroup;

        SveltoDictionaryKeyValueEnumerator<ExclusiveGroupStruct, uint,
                ManagedStrategy<SveltoDictionaryNode<ExclusiveGroupStruct>>, ManagedStrategy<uint>,
                ManagedStrategy<int>>
            _lastNumberEntitiesCreatedPerGroup;
    }

    struct GroupInfo
    {
        public ExclusiveGroupStruct                                  group;
        public FasterDictionary<ComponentID, ITypeSafeDictionary> components;
    }
}