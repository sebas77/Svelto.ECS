using System;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary<TValue> : ITypeSafeDictionary where TValue : IEntityComponent
    {
        void Add(uint egidEntityId, in TValue entityComponent);
        
        bool       TryGetValue(uint entityId, out TValue item);
        ref TValue GetOrAdd(uint idEntityId);

        IBuffer<TValue> GetValues(out uint count);
        ref TValue      GetDirectValueByRef(uint key);
        ref TValue      GetValueByRef(uint key);
        EntityIDs       entityIDs { get; }
    }

    public interface ITypeSafeDictionary : IDisposable
    {
        int                 count { get; }
        
        ITypeSafeDictionary Create();

        void AddEntitiesToDictionary
        (ITypeSafeDictionary toDictionary, ExclusiveGroupStruct groupId, in EnginesRoot.EntityReferenceMap entityLocator);
        void RemoveEntitiesFromDictionary(FasterList<(uint, string)> infosToProcess);
        void SwapEntitiesBetweenDictionaries(FasterList<(uint, uint, string)> infosToProcess,
         ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup, ITypeSafeDictionary toComponentsDictionary);
        
        //------------

        //This is now obsolete, but I cannot mark it as such because it's heavily used by legacy projects
        void ExecuteEnginesAddCallbacks
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAdd>>> entityComponentEnginesDb
       , ITypeSafeDictionary destinationDatabase, ExclusiveGroupStruct toGroup, in PlatformProfiler profiler);
        //Version to use
        void ExecuteEnginesAddEntityCallbacksFast(
         FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAddEx>>> reactiveEnginesAdd,
         ExclusiveGroupStruct groupID, (uint, uint) rangeOfSubmittedEntitiesIndicies, in PlatformProfiler profiler);

        //------------
        
        //This is now obsolete, but I cannot mark it as such because it's heavily used by legacy projects
        void ExecuteEnginesSwapCallbacks(FasterList<(uint, uint, string)> infosToProcess,
         FasterList<ReactEngineContainer<IReactOnSwap>> reactiveEnginesSwap, ExclusiveGroupStruct fromGroup,
         ExclusiveGroupStruct toGroup, in PlatformProfiler sampler);
        //Version to use
        void ExecuteEnginesSwapCallbacksFast(FasterList<ReactEngineContainer<IReactOnSwapEx>> reactiveEnginesSwap,
         ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup, (uint, uint) rangeOfSubmittedEntitiesIndicies,
         in PlatformProfiler sampler);
        
        //------------
        
        //This is now obsolete, but I cannot mark it as such because it's heavily used by legacy projects
        void ExecuteEnginesRemoveCallbacks(FasterList<(uint, string)> infosToProcess,
         FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveEnginesRemove,
         ExclusiveGroupStruct fromGroup, in PlatformProfiler sampler);
        //Version to use
        void ExecuteEnginesRemoveCallbacksFast(FasterList<ReactEngineContainer<IReactOnRemoveEx>> reactiveEnginesRemoveEx,
         ExclusiveGroupStruct fromGroup, (uint, uint) rangeOfSubmittedEntitiesIndicies,
         in PlatformProfiler sampler);
        
        //------------

        void ExecuteEnginesSwapCallbacks_Group(
         FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwap>>> reactiveEnginesSwap,
         FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwapEx>>> reactiveEnginesSwapEx,
         ITypeSafeDictionary toEntitiesDictionary, ExclusiveGroupStruct fromGroupId, ExclusiveGroupStruct toGroupId,
         in PlatformProfiler platformProfiler);
        void ExecuteEnginesRemoveCallbacks_Group(
         FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> engines,
         FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemoveEx>>> reactiveEnginesRemoveEx,
         ExclusiveGroupStruct @group, in PlatformProfiler profiler);
        void ExecuteEnginesDisposeCallbacks_Group
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnDispose>>> engines
       , ExclusiveGroupStruct group, in PlatformProfiler profiler);

        void IncreaseCapacityBy(uint size);
        void EnsureCapacity(uint size);
        void Trim();
        void Clear();
        bool Has(uint key);
        bool ContainsKey(uint egidEntityId);
        uint GetIndex(uint valueEntityId);
        bool TryFindIndex(uint entityGidEntityId, out uint index);

        void KeysEvaluator(System.Action<uint> action);
    }
}