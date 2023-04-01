using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto.ECS
{
    class EntitiesOperations
    {
        /// <summary>
        /// Todo: need to go back here and add a ton of comments
        /// </summary>
        public EntitiesOperations()
        {
            _thisSubmissionInfo.Init();
            _lastSubmittedInfo.Init();
            _newGroupDictionary = NewGroupDictionary;
            _newGroupsDictionary = NewGroupsDictionary;
            _recycleDictionary = RecycleDictionary;
            _newList = NewList;
            _clearList = ClearList;
            _newGroupsDictionaryWithCaller = NewGroupsDictionaryWithCaller;
            _recycleGroupDictionaryWithCaller = RecycleGroupDictionaryWithCaller;
            _recycleDicitionaryWithCaller = RecycleDicitionaryWithCaller;
            _newListWithCaller = NewListWithCaller;
            _clearListWithCaller = ClearListWithCaller;
        }

        public void QueueRemoveGroupOperation(ExclusiveBuildGroup groupID, string caller)
        {
            _thisSubmissionInfo._groupsToRemove.Add((groupID, caller));
        }

        public void QueueRemoveOperation(EGID entityEgid, IComponentBuilder[] componentBuilders, string caller)
        {
            _thisSubmissionInfo._entitiesRemoved.Add(entityEgid);
            
            //todo: limit the number of dictionaries that can be cached 
            //recycle or create dictionaries of components per group
            var removedComponentsPerType = _thisSubmissionInfo._currentRemoveEntitiesOperations.RecycleOrAdd(
                entityEgid.groupID, _newGroupsDictionary, _recycleDictionary);

            foreach (var operation in componentBuilders)
            {
                removedComponentsPerType //recycle or create dictionaries per component type
                       .RecycleOrAdd(operation.getComponentID, _newList, _clearList)
                        //add entity to remove
                       .Add((entityEgid.entityID, caller));
            }
        }

        public void QueueSwapGroupOperation(ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID, string caller)
        {
            _thisSubmissionInfo._groupsToSwap.Add((fromGroupID, toGroupID, caller));
        }

        public void QueueSwapOperation(EGID fromID, EGID toID, IComponentBuilder[] componentBuilders, string caller)
        {
            _thisSubmissionInfo._entitiesSwapped.Add((fromID, toID));

            //todo: limit the number of dictionaries that can be cached 
            //recycle or create dictionaries of components per group
            
            //Get the dictionary that holds the entities that are swapping from fromID
            var swappedComponentsPerType = _thisSubmissionInfo._currentSwapEntitiesOperations.RecycleOrAdd(
                fromID.groupID, _newGroupsDictionaryWithCaller, _recycleGroupDictionaryWithCaller);

            var componentBuildersLength = componentBuilders.Length - 1;
            for (var index = componentBuildersLength; index >= 0; index--)
            {
                 var operation = componentBuilders[index];
                 
                 //Get the dictionary for each component that holds the list of entities to swap
                swappedComponentsPerType //recycle or create dictionaries per component type
                       .RecycleOrAdd(operation.getComponentID, _newGroupDictionary, _recycleDicitionaryWithCaller)
                        //recycle or create list of entities to swap
                       .RecycleOrAdd(toID.groupID, _newListWithCaller, _clearListWithCaller)
                        //add entity to swap
                       .Add((fromID.entityID, toID.entityID, caller));
            }
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool AnyOperationQueued()
        {
            return _thisSubmissionInfo.AnyOperationQueued();
        }

        public void ExecuteRemoveAndSwappingOperations(Action<FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID,
            FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>>, FasterList<(EGID, EGID)>,
            EnginesRoot> swapEntities, Action<FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, FasterList<(uint, string)>>>,
            FasterList<EGID>, EnginesRoot> removeEntities, Action<ExclusiveGroupStruct, EnginesRoot> removeGroup, 
            Action<ExclusiveGroupStruct, ExclusiveGroupStruct, EnginesRoot> swapGroup, EnginesRoot enginesRoot)
        {
            (_thisSubmissionInfo, _lastSubmittedInfo) = (_lastSubmittedInfo, _thisSubmissionInfo);

            /// todo: entity references should be updated before calling all the methods to avoid callbacks handling
            /// references that should be marked as invalid.
            foreach (var (group, caller) in _lastSubmittedInfo._groupsToRemove)
                try
                {
                    removeGroup(group, enginesRoot);
                }
                catch
                {
                    var str = "Crash while removing a whole group on ".FastConcat(group.ToString())
                                                                      .FastConcat(" from : ", caller);

                    Console.LogError(str);

                    throw;
                }

            foreach (var (fromGroup, toGroup, caller) in _lastSubmittedInfo._groupsToSwap)
                try
                {
                    swapGroup(fromGroup, toGroup, enginesRoot);
                }
                catch
                {
                    var str = "Crash while swapping a whole group on "
                             .FastConcat(fromGroup.ToString(), " ", toGroup.ToString()).FastConcat(" from : ", caller);

                    Console.LogError(str);

                    throw;
                }

            if (_lastSubmittedInfo._entitiesSwapped.count > 0)
                swapEntities(_lastSubmittedInfo._currentSwapEntitiesOperations, _lastSubmittedInfo._entitiesSwapped
                           , enginesRoot);

            if (_lastSubmittedInfo._entitiesRemoved.count > 0)
                removeEntities(_lastSubmittedInfo._currentRemoveEntitiesOperations, _lastSubmittedInfo._entitiesRemoved
                             , enginesRoot);

            _lastSubmittedInfo.Clear();
        }
        
        FasterDictionary<ComponentID, FasterList<(uint, string)>> NewGroupsDictionary()
        {
            return new FasterDictionary<ComponentID, FasterList<(uint, string)>>();
        }
        
        void RecycleDictionary(ref FasterDictionary<ComponentID, FasterList<(uint, string)>> recycled)
        {
            recycled.Recycle();
        }

        FasterList<(uint, string)> NewList()
        {
            return new FasterList<(uint, string)>();
        }

        void ClearList(ref FasterList<(uint, string)> target)
        {
            target.Clear();
        }

        void RecycleDicitionaryWithCaller(ref FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>> target)
        {
            target.Recycle();
        }

        void ClearListWithCaller(ref FasterList<(uint, uint, string)> target)
        {
            target.Clear();
        }

        FasterList<(uint, uint, string)> NewListWithCaller()
        {
            return new FasterList<(uint, uint, string)>();
        }

        FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>> NewGroupsDictionaryWithCaller()
        {
            return new FasterDictionary<ComponentID, //add case
                FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>();
        }

        void RecycleGroupDictionaryWithCaller(ref FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>> recycled)
        {
            recycled.Recycle();
        }

        FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>> NewGroupDictionary()
        {
            return new FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>();
        }

        struct Info
        {
                                      //from group         //actual component type      
            internal FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID,
                                     // to group ID        //entityIDs , debugInfo
                    FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>>
                _currentSwapEntitiesOperations;

            internal FasterDictionary<ExclusiveGroupStruct,
                FasterDictionary<ComponentID, FasterList<(uint, string)>>> _currentRemoveEntitiesOperations;

            internal FasterList<(EGID, EGID)>                                       _entitiesSwapped;
            internal FasterList<EGID>                                               _entitiesRemoved;
            public   FasterList<(ExclusiveBuildGroup, ExclusiveBuildGroup, string)> _groupsToSwap;
            public   FasterList<(ExclusiveBuildGroup, string)>                      _groupsToRemove;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal bool AnyOperationQueued()
            {
                return _entitiesSwapped.count > 0 || _entitiesRemoved.count > 0 || _groupsToSwap.count > 0
                    || _groupsToRemove.count > 0;
            }

            internal void Clear()
            {
                _currentSwapEntitiesOperations.Recycle();
                _currentRemoveEntitiesOperations.Recycle();
                _entitiesSwapped.Clear();
                _entitiesRemoved.Clear();
                _groupsToRemove.Clear();
                _groupsToSwap.Clear();
            }

            internal void Init()
            {
                _entitiesSwapped = new FasterList<(EGID, EGID)>();
                _entitiesRemoved = new FasterList<EGID>();
                _groupsToRemove  = new FasterList<(ExclusiveBuildGroup, string)>();
                _groupsToSwap    = new FasterList<(ExclusiveBuildGroup, ExclusiveBuildGroup, string)>();

                _currentSwapEntitiesOperations =
                    new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID,
                        FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>>();
                _currentRemoveEntitiesOperations =
                    new FasterDictionary<ExclusiveGroupStruct,
                        FasterDictionary<ComponentID, FasterList<(uint, string)>>>();
            }
        }

        Info _lastSubmittedInfo;
        Info _thisSubmissionInfo;

        readonly Func<FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>> _newGroupDictionary;
        readonly Func<FasterDictionary<ComponentID, FasterList<(uint, string)>>> _newGroupsDictionary;
        readonly ActionRef<FasterDictionary<ComponentID, FasterList<(uint, string)>>> _recycleDictionary;
        readonly Func<FasterList<(uint, string)>> _newList;
        readonly ActionRef<FasterList<(uint, string)>> _clearList;
        readonly Func<FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>> _newGroupsDictionaryWithCaller;
        readonly ActionRef<FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>> _recycleGroupDictionaryWithCaller;
        readonly ActionRef<FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>> _recycleDicitionaryWithCaller;
        readonly Func<FasterList<(uint, uint, string)>> _newListWithCaller;
        readonly ActionRef<FasterList<(uint, uint, string)>> _clearListWithCaller;
    }
}