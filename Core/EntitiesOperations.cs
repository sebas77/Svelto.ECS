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

        public void QueueRemoveOperation(EGID fromEgid, IComponentBuilder[] componentBuilders, string caller)
        {
            // Check if the entity is already queued for removal
            if (_thisSubmissionInfo._entitiesRemoved.Contains(fromEgid))
            {
                // If it is, skip the rest of the function
                return;
            }
            
            _thisSubmissionInfo._entitiesRemoved.Add(fromEgid);
            RevertSwapOperationIfPreviouslyQueued(fromEgid);

            //todo: limit the number of dictionaries that can be cached 
            //recycle or create dictionaries of components per group
            var removedComponentsPerType = _thisSubmissionInfo._currentRemoveEntitiesOperations.RecycleOrAdd(
                fromEgid.groupID, _newGroupsDictionary, _recycleDictionary);

            foreach (var operation in componentBuilders)
            {
                removedComponentsPerType //recycle or create dictionaries per component type
                       .RecycleOrAdd(operation.getComponentID, _newList, _clearList)
                        //add entity to remove
                       .Add((fromEgid.entityID, caller));
            }

            void RevertSwapOperationIfPreviouslyQueued(EGID fromEgid)
            {
                if (_thisSubmissionInfo._entitiesSwapped.Remove(fromEgid, out (EGID fromEgid, EGID toEgid) val)) //Remove supersedes swap, check comment in IEntityFunctions.cs
                {
                    var swappedComponentsPerType = _thisSubmissionInfo._currentSwapEntitiesOperations[fromEgid.groupID];

                    var componentBuildersLength = componentBuilders.Length - 1;

                    for (var index = componentBuildersLength; index >= 0; index--)
                    {
                        var operation = componentBuilders[index];

                        //todo: maybe the order of swappedComponentsPerType should be fromID, toGroupID, componentID 
                        swappedComponentsPerType[operation.getComponentID][val.toEgid.groupID].Remove(fromEgid.entityID);
                    }
                }
            }
        }

        public void QueueSwapGroupOperation(ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID, string caller)
        {
            _thisSubmissionInfo._groupsToSwap.Add((fromGroupID, toGroupID, caller));
        }

        public void QueueSwapOperation(EGID fromEGID, EGID toEGID, IComponentBuilder[] componentBuilders, string caller)
        {
            _thisSubmissionInfo._entitiesSwapped.Add(fromEGID, (fromEGID, toEGID));

            //todo: limit the number of dictionaries that can be cached 

            //Get (or create) the dictionary that holds the entities that are swapping from fromEGID group
            var swappedComponentsPerType = _thisSubmissionInfo._currentSwapEntitiesOperations.RecycleOrAdd(
                fromEGID.groupID, _newGroupsDictionaryWithCaller, _recycleGroupDictionaryWithCaller);

            var componentBuildersLength = componentBuilders.Length - 1;
            //for each component of the entity that is swapping
            for (var index = componentBuildersLength; index >= 0; index--)
            {
                var operation = componentBuilders[index];

                //Get the dictionary for each component that holds the list of entities to swap
                swappedComponentsPerType
                        //recycle or create dictionaries per component type
                       .RecycleOrAdd(operation.getComponentID, _newGroupDictionary, _recycleDicitionaryWithCaller)
                        //recycle or create list of entities to swap
                       .RecycleOrAdd(toEGID.groupID, _newListWithCaller, _clearListWithCaller)
                        //add entity to swap
                       .Add(fromEGID.entityID, new SwapInfo(fromEGID.entityID, toEGID.entityID, caller));
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool AnyOperationQueued()
        {
            return _thisSubmissionInfo.AnyOperationQueued();
        }

        public void ExecuteRemoveAndSwappingOperations(
            Action<FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>>>>, FasterDictionary<EGID, (EGID, EGID)>, EnginesRoot> swapEntities, Action<FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, FasterList<(uint, string)>>>,
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
                swapEntities(_lastSubmittedInfo._currentSwapEntitiesOperations, _lastSubmittedInfo._entitiesSwapped, enginesRoot);

            if (_lastSubmittedInfo._entitiesRemoved.count > 0)
                removeEntities(
                    _lastSubmittedInfo._currentRemoveEntitiesOperations, _lastSubmittedInfo._entitiesRemoved
                  , enginesRoot);

            _lastSubmittedInfo.Clear();
        }

        static FasterDictionary<ComponentID, FasterList<(uint, string)>> NewGroupsDictionary()
        {
            return new FasterDictionary<ComponentID, FasterList<(uint, string)>>();
        }

        static void RecycleDictionary(ref FasterDictionary<ComponentID, FasterList<(uint, string)>> recycled)
        {
            recycled.Recycle();
        }

        static FasterList<(uint, string)> NewList()
        {
            return new FasterList<(uint, string)>();
        }

        static void ClearList(ref FasterList<(uint, string)> target)
        {
            target.Clear();
        }

        static void RecycleDicitionaryWithCaller(ref FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>> target)
        {
            target.Recycle();
        }

        static void ClearListWithCaller(ref FasterDictionary<uint, SwapInfo> target)
        {
            target.Clear();
        }

        static FasterDictionary<uint, SwapInfo> NewListWithCaller()
        {
            return new FasterDictionary<uint, SwapInfo>();
        }

        static FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>>>
                NewGroupsDictionaryWithCaller()
        {
            return new FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>>>();
        }

        static void RecycleGroupDictionaryWithCaller(
            ref FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>>> recycled)
        {
            recycled.Recycle();
        }

        static FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>> NewGroupDictionary()
        {
            return new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>>();
        }

        struct Info
        {
            //from group         //actual component type      
            internal FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>>>> _currentSwapEntitiesOperations;

            internal FasterDictionary<ExclusiveGroupStruct,
                FasterDictionary<ComponentID, FasterList<(uint, string)>>> _currentRemoveEntitiesOperations;

            internal FasterDictionary<EGID, (EGID fromEgid, EGID toEgid)> _entitiesSwapped;
            internal FasterList<EGID> _entitiesRemoved;
            public FasterList<(ExclusiveBuildGroup, ExclusiveBuildGroup, string)> _groupsToSwap;
            public FasterList<(ExclusiveBuildGroup, string)> _groupsToRemove;

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
                _entitiesSwapped = new FasterDictionary<EGID, (EGID fromEgid, EGID toEgid)>();
                _entitiesRemoved = new FasterList<EGID>();
                _groupsToRemove = new FasterList<(ExclusiveBuildGroup, string)>();
                _groupsToSwap = new FasterList<(ExclusiveBuildGroup, ExclusiveBuildGroup, string)>();

                _currentSwapEntitiesOperations =
                        new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<ComponentID,
                            FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>>>>();
                _currentRemoveEntitiesOperations =
                        new FasterDictionary<ExclusiveGroupStruct,
                            FasterDictionary<ComponentID, FasterList<(uint, string)>>>();
            }
        }

        Info _lastSubmittedInfo;
        Info _thisSubmissionInfo;

        readonly Func<FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>>> _newGroupDictionary;
        readonly Func<FasterDictionary<ComponentID, FasterList<(uint, string)>>> _newGroupsDictionary;
        readonly ActionRef<FasterDictionary<ComponentID, FasterList<(uint, string)>>> _recycleDictionary;
        readonly Func<FasterList<(uint, string)>> _newList;
        readonly ActionRef<FasterList<(uint, string)>> _clearList;

        readonly Func<FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>>>>
                _newGroupsDictionaryWithCaller;

        readonly ActionRef<FasterDictionary<ComponentID, FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>>>>
                _recycleGroupDictionaryWithCaller;

        readonly ActionRef<FasterDictionary<ExclusiveGroupStruct, FasterDictionary<uint, SwapInfo>>> _recycleDicitionaryWithCaller;
        readonly Func<FasterDictionary<uint, SwapInfo>> _newListWithCaller;
        readonly ActionRef<FasterDictionary<uint, SwapInfo>> _clearListWithCaller;
    }

    public struct SwapInfo
    {
        public uint fromID; //to do this information should be redundant, try to remove it
        public uint toID;
        public uint toIndex;
        public string trace;

        public SwapInfo(uint fromEgidEntityId, uint toEgidEntityId, string s)
        {
            fromID = fromEgidEntityId;
            toID = toEgidEntityId;
            toIndex = 0;
            trace = s;
        }

        public void Deconstruct(out uint fromID, out uint toID, out uint toIndex, out string caller)
        {
            fromID = this.fromID;
            toID = this.toID;
            toIndex = this.toIndex;
            caller = this.trace;
        }
        
        public void Deconstruct(out uint fromID, out uint toID, out string caller)
        {
            fromID = this.fromID;
            toID = this.toID;
            caller = this.trace;
        }
    }
}