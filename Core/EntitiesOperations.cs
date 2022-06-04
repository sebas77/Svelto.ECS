using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    class EntitiesOperations
    {
        public EntitiesOperations()
        {
            _thisSubmissionInfo.Init();
            _lastSubmittedInfo.Init();
        }

        public void AddSwapOperation(EGID fromID, EGID toID, IComponentBuilder[] componentBuilders, string caller)
        {
            _thisSubmissionInfo._entitiesSwapped.Add((fromID, toID));

            //todo: limit the number of dictionaries that can be cached 
            //recycle or create dictionaries of components per group
            var swappedComponentsPerType = _thisSubmissionInfo._currentSwapEntitiesOperations.RecycleOrAdd(
                fromID.groupID,
                () => new FasterDictionary<RefWrapperType,
                    FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>(),
                (ref FasterDictionary<RefWrapperType, FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>> recycled) =>
                    recycled.FastClear());

            foreach (IComponentBuilder operation in componentBuilders)
            {
                swappedComponentsPerType
                    //recycle or create dictionaries per component type
                   .RecycleOrAdd(new RefWrapperType(operation.GetEntityComponentType()),
                        () => new FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>(),
                        (ref FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>> target) =>
                            target.FastClear())
                    //recycle or create list of entities to swap
                   .RecycleOrAdd(toID.groupID, () => new FasterList<(uint, uint, string)>(),
                        (ref FasterList<(uint, uint, string)> target) => target.FastClear())
                    //add entity to swap
                   .Add((fromID.entityID, toID.entityID, caller));
            }
        }

        public void AddRemoveOperation(EGID entityEgid, IComponentBuilder[] componentBuilders, string caller)
        {
            _thisSubmissionInfo._entitiesRemoved.Add(entityEgid);
            //todo: limit the number of dictionaries that can be cached 
            //recycle or create dictionaries of components per group
            var removedComponentsPerType = _thisSubmissionInfo._currentRemoveEntitiesOperations.RecycleOrAdd(
                entityEgid.groupID, () => new FasterDictionary<RefWrapperType, FasterList<(uint, string)>>(),
                (ref FasterDictionary<RefWrapperType, FasterList<(uint, string)>> recycled) => recycled.FastClear());

            foreach (IComponentBuilder operation in componentBuilders)
            {
                removedComponentsPerType
                    //recycle or create dictionaries per component type
                   .RecycleOrAdd(new RefWrapperType(operation.GetEntityComponentType()),
                        () => new FasterList<(uint, string)>(),
                        (ref FasterList<(uint, string)> target) => target.FastClear())
                    //add entity to swap
                   .Add((entityEgid.entityID, caller));
            }
        }

        public void AddRemoveGroupOperation(ExclusiveBuildGroup groupID, string caller)
        {
            _thisSubmissionInfo._groupsToRemove.Add((groupID, caller));
        }

        public void AddSwapGroupOperation(ExclusiveBuildGroup fromGroupID, ExclusiveBuildGroup toGroupID, string caller)
        {
            _thisSubmissionInfo._groupsToSwap.Add((fromGroupID, toGroupID, caller));
        }

        public void ExecuteRemoveAndSwappingOperations(
            Action<FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType,
                    FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>>, FasterList<(EGID, EGID)>
               ,
                EnginesRoot> swapEntities,
            Action<FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType, FasterList<(uint, string)>>>,
                FasterList<EGID>, EnginesRoot> removeEntities, Action<ExclusiveGroupStruct, EnginesRoot> removeGroup,
            Action<ExclusiveGroupStruct, ExclusiveGroupStruct, EnginesRoot> swapGroup, EnginesRoot enginesRoot)
        {
            (_thisSubmissionInfo, _lastSubmittedInfo) = (_lastSubmittedInfo, _thisSubmissionInfo);

            /// todo: entity references should be updated before calling all the methods to avoid callbacks handling
            /// references that should be marked as invalid.
            foreach (var (group, caller) in _lastSubmittedInfo._groupsToRemove)
            {
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
            }

            foreach (var (fromGroup, toGroup, caller) in _lastSubmittedInfo._groupsToSwap)
            {
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
            }

            if (_lastSubmittedInfo._entitiesSwapped.count > 0)
                swapEntities(_lastSubmittedInfo._currentSwapEntitiesOperations, _lastSubmittedInfo._entitiesSwapped,
                    enginesRoot);

            if (_lastSubmittedInfo._entitiesRemoved.count > 0)
                removeEntities(_lastSubmittedInfo._currentRemoveEntitiesOperations, _lastSubmittedInfo._entitiesRemoved,
                    enginesRoot);

            _lastSubmittedInfo.Clear();
        }

        public bool AnyOperationQueued() => _thisSubmissionInfo.AnyOperationQueued();

        struct Info
        {
            //from group                          //actual component type      
            internal FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType,
                    // to group ID        //entityIDs , debugInfo
                    FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>>
                _currentSwapEntitiesOperations;

            internal FasterDictionary<ExclusiveGroupStruct,
                FasterDictionary<RefWrapperType, FasterList<(uint, string)>>> _currentRemoveEntitiesOperations;

            internal FasterList<(EGID, EGID)>                                       _entitiesSwapped;
            internal FasterList<EGID>                                               _entitiesRemoved;
            public   FasterList<(ExclusiveBuildGroup, ExclusiveBuildGroup, string)> _groupsToSwap;
            public   FasterList<(ExclusiveBuildGroup, string)>                      _groupsToRemove;

            internal bool AnyOperationQueued() =>
                _entitiesSwapped.count > 0 || _entitiesRemoved.count > 0 || _groupsToSwap.count > 0 ||
                _groupsToRemove.count > 0;

            internal void Clear()
            {
                _currentSwapEntitiesOperations.FastClear();
                _currentRemoveEntitiesOperations.FastClear();
                _entitiesSwapped.FastClear();
                _entitiesRemoved.FastClear();
                _groupsToRemove.FastClear();
                _groupsToSwap.FastClear();
            }

            internal void Init()
            {
                _entitiesSwapped = new FasterList<(EGID, EGID)>();
                _entitiesRemoved = new FasterList<EGID>();
                _groupsToRemove  = new FasterList<(ExclusiveBuildGroup, string)>();
                _groupsToSwap    = new FasterList<(ExclusiveBuildGroup, ExclusiveBuildGroup, string)>();

                _currentSwapEntitiesOperations =
                    new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapperType,
                        FasterDictionary<ExclusiveGroupStruct, FasterList<(uint, uint, string)>>>>();
                _currentRemoveEntitiesOperations =
                    new FasterDictionary<ExclusiveGroupStruct,
                        FasterDictionary<RefWrapperType, FasterList<(uint, string)>>>();
            }
        }

        Info _thisSubmissionInfo;
        Info _lastSubmittedInfo;
    }
}