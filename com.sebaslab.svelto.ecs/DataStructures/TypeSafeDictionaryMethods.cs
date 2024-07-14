using System;
using System.Runtime.CompilerServices;
using DBC.ECS;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public static class TypeSafeDictionaryMethods
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddEntitiesToDictionary<Strategy1, Strategy2, Strategy3, TValue>(
            in SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
          , ITypeSafeDictionary<TValue> toDic
#if SLOW_SVELTO_SUBMISSION
          , in EnginesRoot.EntityReferenceMap entityLocator
#endif
          , ExclusiveGroupStruct toGroupID)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
                where TValue : struct, _IInternalEntityComponent
        {
            foreach (var tuple in fromDictionary)
            {
#if SLOW_SVELTO_SUBMISSION
                var egid = new EGID(tuple.key, toGroupID);

                if (SlowSubmissionInfo<TValue>.hasEgid)
                    SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref tuple.value, egid);

                if (SlowSubmissionInfo<TValue>.hasReference)
                    SetEGIDWithoutBoxing<TValue>.SetRefWithoutBoxing(
                        ref tuple.value,
                        entityLocator.GetEntityReference(egid));
#endif
#if DEBUG && !PROFILE_SVELTO                
                try
                {
#endif                    
                    toDic.Add(tuple.key, tuple.value);
#if DEBUG && !PROFILE_SVELTO                    
                }
                catch (Exception e)
                {
                    Console.LogException(
                        e,
                        "trying to add an EntityComponent with the same ID more than once Entity: ".FastConcat(typeof(TValue).ToString())
                               .FastConcat(", group ").FastConcat(toGroupID.ToName()).FastConcat(", id ").FastConcat(tuple.key));

                    throw;
                }
#endif                
#if PARANOID_CHECK && SLOW_SVELTO_SUBMISSION
                        DBC.ECS.Check.Ensure(_hasEgid == false || ((INeedEGID)fromDictionary[egid.entityID]).ID == egid, "impossible situation happened during swap");
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesAddCallbacks<Strategy1, Strategy2, Strategy3, TValue>(
            ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
          , ITypeSafeDictionary<TValue> todic, ExclusiveGroupStruct togroup
          , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAdd>>> entitycomponentenginesdb
          , in PlatformProfiler sampler)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
                where TValue : struct, _IInternalEntityComponent
        {
            if (entitycomponentenginesdb.TryGetValue(
                    ComponentTypeID<TValue>.id
                  , out var entityComponentsEngines))
            {
                if (entityComponentsEngines.count == 0)
                    return;

                var dictionaryKeyEnumerator = fromDictionary.unsafeKeys;
                var count = fromDictionary.count;

                for (var i = 0; i < count; ++i)
                    try
                    {
                        var key = dictionaryKeyEnumerator[i].key;
                        ref var entity = ref todic.GetValueByRef(key);
                        var egid = new EGID(key, togroup);
                        //get all the engines linked to TValue
                        for (var j = 0; j < entityComponentsEngines.count; j++)
                            using (sampler.Sample(entityComponentsEngines[j].name))
                            {
#pragma warning disable CS0612
                                ((IReactOnAdd<TValue>)entityComponentsEngines[j].engine).Add(ref entity, egid);
#pragma warning restore CS0612
                            }
                    }
                    catch (Exception e)
                    {
                        Console.LogException(e, "Code crashed inside Add callback with Type ".FastConcat(TypeCache<TValue>.name));

                        throw;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesDisposeCallbacks_Group<Strategy1, Strategy2, Strategy3, TValue>(
            ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
          , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDispose>>> reactiveEnginesDispose
          , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnDisposeEx>>> reactiveEnginesDisposeEx
          , IEntityIDs entityids, ExclusiveGroupStruct group, in PlatformProfiler sampler)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
                where TValue : struct, _IInternalEntityComponent
        {
            if (reactiveEnginesDispose.TryGetValue(ComponentTypeID<TValue>.id, out var entityComponentsEngines) == true)
            {
                var resultCount = entityComponentsEngines.count;
                for (var i = 0; i < resultCount; i++)
                    try
                    {
                        using (sampler.Sample(entityComponentsEngines[i].name))
                        {
                            foreach (var value in fromDictionary)
                            {
                                ref var entity = ref value.value;
                                var egid = new EGID(value.key, group);
#pragma warning disable CS0618
                                var reactOnRemove = (IReactOnDispose<TValue>)entityComponentsEngines[i].engine;
#pragma warning restore CS0618
                                reactOnRemove.Remove(ref entity, egid);
                            }
                        }
                    }
                    catch
                    {
                        Console.LogError("Code crashed inside Remove callback ".FastConcat(entityComponentsEngines[i].name));

                        throw;
                    }
            }

            if (reactiveEnginesDisposeEx.TryGetValue(ComponentTypeID<TValue>.id, out var reactiveEnginesDisposeExPerType))
            {
                var count = fromDictionary.count;
                var enginesCount = reactiveEnginesDisposeExPerType.count;

                if (count > 0)
                {
                    for (var i = 0; i < enginesCount; i++)
                    {
                        try
                        {
                            using (sampler.Sample(reactiveEnginesDisposeExPerType[i].name))
                            {
                                ((IReactOnDisposeEx<TValue>)reactiveEnginesDisposeExPerType[i].engine).Remove(
                                    (0, (uint)count)
                                  , new EntityCollection<TValue>(fromDictionary.UnsafeGetValues(out _), entityids, (uint)count), group);
                            }
                        }
                        catch
                        {
                            Console.LogError("Code crashed inside Remove callback ".FastConcat(reactiveEnginesDisposeExPerType[i].name));

                            throw;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesRemoveCallbacks<Strategy1, Strategy2, Strategy3, TValue>(FasterList<(uint, string)> infostoprocess
          , ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
          , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveenginesremove
          , ExclusiveGroupStruct fromgroup, in PlatformProfiler profiler)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
                where TValue : struct, _IInternalEntityComponent
        {
            if (reactiveenginesremove.TryGetValue(
                    ComponentTypeID<TValue>.id
                  , out var entityComponentsEngines))
            {
                if (entityComponentsEngines.count == 0)
                    return;

                var iterations = infostoprocess.count;

                for (var i = 0; i < iterations; i++)
                {
                    var (entityID, trace) = infostoprocess[i];
                    try
                    {
                        ref var entity = ref fromDictionary.GetValueByRef(entityID);
                        var egid = new EGID(entityID, fromgroup);

                        for (var j = 0; j < entityComponentsEngines.count; j++)
                            using (profiler.Sample(entityComponentsEngines[j].name))
                            {
#pragma warning disable CS0612
                                ((IReactOnRemove<TValue>)entityComponentsEngines[j].engine).Remove(ref entity, egid);
#pragma warning restore CS0612
                            }
                    }
                    catch
                    {
                        var str = "Crash while executing Remove Entity callback on ".FastConcat(TypeCache<TValue>.name)
                               .FastConcat(" from : ", trace);

                        Console.LogError(str);

                        throw;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesRemoveCallbacks_Group<Strategy1, Strategy2, Strategy3, TValue>(
            ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
          , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveenginesremove
          , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnRemoveEx>>> reactiveenginesremoveex
          , IEntityIDs entityids, ExclusiveGroupStruct group, in PlatformProfiler sampler)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
                where TValue : struct, _IInternalEntityComponent
        {
            if (reactiveenginesremove.TryGetValue(
                    ComponentTypeID<TValue>.id
                  , out var reactiveEnginesRemovePerType))
            {
                var enginesCount = reactiveEnginesRemovePerType.count;

                for (var i = 0; i < enginesCount; i++)
                    try
                    {
                        foreach (var value in fromDictionary)
                        {
                            ref var entity = ref value.value;
                            var egid = new EGID(value.key, group);

                            using (sampler.Sample(reactiveEnginesRemovePerType[i].name))
                            {
#pragma warning disable CS0612
                                ((IReactOnRemove<TValue>)reactiveEnginesRemovePerType[i].engine).Remove(
#pragma warning restore CS0612
                                    ref entity, egid);
                            }
                        }
                    }
                    catch
                    {
                        Console.LogError("Code crashed inside Remove callback ".FastConcat(reactiveEnginesRemovePerType[i].name));

                        throw;
                    }
            }

            if (reactiveenginesremoveex.TryGetValue(
                    ComponentTypeID<TValue>.id
                  , out var reactiveEnginesRemoveExPerType))
            {
                var count  = fromDictionary.count;
                var enginesCount = reactiveEnginesRemoveExPerType.count;

                for (var i = 0; i < enginesCount; i++)
                    try
                    {
                        using (sampler.Sample(reactiveEnginesRemoveExPerType[i].name))
                        {
                            ((IReactOnRemoveEx<TValue>)reactiveEnginesRemoveExPerType[i].engine).Remove(
                                (0, (uint)count)
                              , new EntityCollection<TValue>(
                                    fromDictionary.UnsafeGetValues(out _), entityids
                                  , (uint)count), group);
                        }
                    }
                    catch
                    {
                        Console.LogError("Code crashed inside Remove callback ".FastConcat(reactiveEnginesRemoveExPerType[i].name));

                        throw;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesSwapCallbacks<Strategy1, Strategy2, Strategy3, TValue>(FasterDictionary<uint, SwapInfo> infostoprocess
          , ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
          , FasterList<ReactEngineContainer<IReactOnSwap>> reactiveenginesswap, ExclusiveGroupStruct togroup
          , ExclusiveGroupStruct fromgroup, in PlatformProfiler sampler)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
                where TValue : struct, _IInternalEntityComponent
        {
            if (reactiveenginesswap.count == 0)
                return;

            var iterations = infostoprocess.count;

            var infostoprocessUnsafeValues = infostoprocess.unsafeValues;

            for (var i = 0; i < iterations; i++)
            {
                var (fromEntityID, toEntityID, trace) = infostoprocessUnsafeValues[i];

                try
                {
                    ref var entityComponent = ref fromDictionary.GetValueByRef(toEntityID);
                    var newEgid = new EGID(toEntityID, togroup);
                    for (var j = 0; j < reactiveenginesswap.count; j++)
                        using (sampler.Sample(reactiveenginesswap[j].name))
                        {
#pragma warning disable CS0612
#pragma warning disable CS0618
                            ((IReactOnSwap<TValue>)reactiveenginesswap[j].engine).MovedTo(
#pragma warning restore CS0618
#pragma warning restore CS0612
                                ref entityComponent, fromgroup, newEgid);
                        }
                }
                catch
                {
                    var str = "Crash while executing Swap Entity callback on ".FastConcat(TypeCache<TValue>.name)
                           .FastConcat(" from : ", trace);

                    Console.LogError(str);

                    throw;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesSwapCallbacks_Group<Strategy1, Strategy2, Strategy3, TValue>(
            ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
          , ITypeSafeDictionary<TValue> toDic, ExclusiveGroupStruct togroup, ExclusiveGroupStruct fromgroup
          , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwap>>> reactiveenginesswap
          , FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnSwapEx>>> reactiveenginesswapex
          , IEntityIDs entityids, in PlatformProfiler sampler)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
                where TValue : struct, _IInternalEntityComponent
        {
            //get all the engines linked to TValue
            if (!reactiveenginesswap.TryGetValue(
                    ComponentTypeID<TValue>.id
                  , out var reactiveEnginesSwapPerType))
                return;

            var componentsEnginesCount = reactiveEnginesSwapPerType.count;

            for (var i = 0; i < componentsEnginesCount; i++)
                try
                {
                    foreach (var value in fromDictionary)
                    {
                        ref var entityComponent = ref toDic.GetValueByRef(value.key);
                        var newEgid = new EGID(value.key, togroup);

                        using (sampler.Sample(reactiveEnginesSwapPerType[i].name))
                        {
#pragma warning disable CS0612
#pragma warning disable CS0618
                            ((IReactOnSwap<TValue>)reactiveEnginesSwapPerType[i].engine).MovedTo(
#pragma warning restore CS0618
#pragma warning restore CS0612
                                ref entityComponent, fromgroup, newEgid);
                        }
                    }
                }
                catch (Exception)
                {
                    Console.LogError("Code crashed inside MoveTo callback ".FastConcat(reactiveEnginesSwapPerType[i].name));

                    throw;
                }

            if (reactiveenginesswapex.TryGetValue(
                    ComponentTypeID<TValue>.id
                  , out var reactiveEnginesRemoveExPerType))
            {
                var enginesCount = reactiveEnginesRemoveExPerType.count;
                var count = fromDictionary.count;

                for (var i = 0; i < enginesCount; i++)
                    try
                    {
                        using (sampler.Sample(reactiveEnginesRemoveExPerType[i].name))
                        {
                            ((IReactOnSwapEx<TValue>)reactiveEnginesRemoveExPerType[i].engine).MovedTo(
                                (0, (uint)count)
                              , new EntityCollection<TValue>(
                                    fromDictionary.UnsafeGetValues(out _), entityids
                                  , (uint)count), fromgroup, togroup);
                        }
                    }
                    catch
                    {
                        Console.LogError("Code crashed inside Remove callback ".FastConcat(reactiveEnginesRemoveExPerType[i].name));

                        throw;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveEntitiesFromDictionary<Strategy1, Strategy2, Strategy3, TValue>(FasterList<(uint, string)> infostoprocess
          , ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
          , FasterDictionary<uint, uint> entityIDsAffectedByRemoveAtSwapBack)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
                where TValue : struct, _IInternalEntityComponent
        {
            var iterations = infostoprocess.count;

            for (var i = 0; i < iterations; i++)
            {
                var (id, trace) = infostoprocess[i];
#if DEBUG && !PROFILE_SVELTO
                try
                {
#endif                    
                    if (fromDictionary.Remove(id, out var index, out var value))
                    {
                        //Note I am doing this to be able to use a range of values even with the 
                        //remove Ex callbacks. Basically I am copying back the deleted value
                        //at the end of the array, so I can use as range count, count + number of deleted entities
                        //I need to swap the keys too to have matching EntityIDs
                        var fromDictionaryCount = fromDictionary.count;
                        var fromDictionaryUnsafeKeys = fromDictionary.unsafeKeys;
                        if (index != fromDictionaryCount)
                        {
                            fromDictionary.unsafeValues[(uint)fromDictionaryCount] = value;
                            fromDictionaryUnsafeKeys[(uint)fromDictionaryCount] = new SveltoDictionaryNode<uint>(id, 0);
                        }

                        //when a component is removed from a component array, a remove swap back happens. This means
                        //that not only we have to remove the index of the component of the entity deleted from the array
                        //but we need also to update the index of the component that has been swapped in the cell
                        //of the deleted component 
                        //entityIDsAffectedByRemoval tracks all the entitiesID of the components that need to be updated
                        //in the filters because their indices in the array changed. 
                        entityIDsAffectedByRemoveAtSwapBack[fromDictionaryUnsafeKeys[index].key] = index;
                    }
#if DEBUG && !PROFILE_SVELTO                    
                }
                catch
                {
                    var str = "Crash while executing Remove Entity operation on ".FastConcat(TypeCache<TValue>.name)
                           .FastConcat(" from : ", trace);

                    Console.LogError(str);

                    throw;
                }
#endif                
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SwapEntitiesBetweenDictionaries<Strategy1, Strategy2, Strategy3, TValue>(in FasterDictionary<uint, SwapInfo> entitiesIDsToSwap
          , ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
          , ITypeSafeDictionary<TValue> toDictionary, ExclusiveGroupStruct fromgroup, ExclusiveGroupStruct togroup
          , FasterDictionary<uint, uint> entityIDsAffectedByRemoveAtSwapBack)
                where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                where Strategy2 : struct, IBufferStrategy<TValue>
                where Strategy3 : struct, IBufferStrategy<int>
                where TValue : struct, _IInternalEntityComponent
        {
            var iterations = entitiesIDsToSwap.count;
            var entitiesToSwapInfo = entitiesIDsToSwap.unsafeValues;
            var fromDictionaryUnsafeKeys = fromDictionary.unsafeKeys;

            for (var i = 0; i < iterations; i++)
            {
                ref SwapInfo swapInfo = ref entitiesToSwapInfo[i];

#if DEBUG && !PROFILE_SVELTO                
                try
                {
#endif                    
                    var fromEntityGid = new EGID(swapInfo.fromID, fromgroup);
                    var toEntityEgid = new EGID(swapInfo.toID, togroup);

                    Check.Require(togroup.isInvalid == false, "Invalid To Group");

                    if (fromDictionary.Remove(fromEntityGid.entityID, out var index, out var value))
                        entityIDsAffectedByRemoveAtSwapBack[fromDictionaryUnsafeKeys[index].key] = index; //after the removal, the entity ad index is the entity that was at the end of the buffer (swapped back). 
                    else
                        Check.Assert(false, "Swapping an entity that doesn't exist");

#if SLOW_SVELTO_SUBMISSION
                    if (SlowSubmissionInfo<TValue>.hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref value, toEntityEgid);
#endif

                    swapInfo.toIndex = toDictionary.Add(toEntityEgid.entityID, value);

#if PARANOID_CHECK
                    DBC.ECS.Check.Ensure(_hasEgid == false || ((INeedEGID)toGroupCasted[toEntityEGID.entityID]).ID == toEntityEGID, "impossible situation happened during swap");
#endif
#if DEBUG && !PROFILE_SVELTO
                }
                catch
                {
                    var str = "Crash while executing Swap Entity operation on ".FastConcat(TypeCache<TValue>.name)
                           .FastConcat(" from : ", swapInfo.trace);

                    Console.LogError(str);

                    throw;
                }
#endif                
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesAddEntityCallbacksFast<TValue>(
            FasterDictionary<ComponentID, FasterList<ReactEngineContainer<IReactOnAddEx>>> fasterDictionary
          , ExclusiveGroupStruct groupId, (uint, uint) rangeTuple, IEntityIDs entityids
          , ITypeSafeDictionary<TValue> typeSafeDictionary, PlatformProfiler profiler)
                where TValue : struct, _IInternalEntityComponent
        {
            //get all the engines linked to TValue
            if (!fasterDictionary.TryGetValue(ComponentTypeID<TValue>.id, out var entityComponentsEngines))
                return;

            for (var i = 0; i < entityComponentsEngines.count; i++)
                try
                {
                    using (profiler.Sample(entityComponentsEngines[i].name))
                    {
                        ((IReactOnAddEx<TValue>)entityComponentsEngines[i].engine).Add(
                            rangeTuple
                          , new EntityCollection<TValue>(typeSafeDictionary.GetValues(out var count), entityids, count)
                          , groupId);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(e, "Code crashed inside Add callback ".FastConcat(entityComponentsEngines[i].name));

                    throw;
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesSwapCallbacksFast<TValue>(
            FasterList<ReactEngineContainer<IReactOnSwapEx>> callbackEngines,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup, IEntityIDs entityids, ITypeSafeDictionary<TValue> typeSafeDictionary
          , (uint, uint) rangeOfSubmittedEntitiesIndicies, PlatformProfiler sampler)
                where TValue : struct, _IInternalEntityComponent
        {
            for (var i = 0; i < callbackEngines.count; i++)
                try
                {
                    using (sampler.Sample(callbackEngines[i].name))
                    {
                        var values = typeSafeDictionary.GetValues(out var count);
                        
                        ((IReactOnSwapEx<TValue>)callbackEngines[i].engine).MovedTo(
                            rangeOfSubmittedEntitiesIndicies, new EntityCollection<TValue>(values, entityids, count), fromGroup, toGroup);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(e, "Code crashed inside Add callback ".FastConcat(callbackEngines[i].name));

                    throw;
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesRemoveCallbacksFast<TValue>(FasterList<ReactEngineContainer<IReactOnRemoveEx>> fasterList,
            ExclusiveGroupStruct exclusiveGroupStruct
          , (uint, uint) valueTuple, IEntityIDs entityids, ITypeSafeDictionary<TValue> typeSafeDictionary
          , PlatformProfiler sampler)
                where TValue : struct, _IInternalEntityComponent
        {
            for (var i = 0; i < fasterList.count; i++)
                try
                {
                    using (sampler.Sample(fasterList[i].name))
                    {
                        ((IReactOnRemoveEx<TValue>)fasterList[i].engine).Remove(
                            valueTuple
                          , new EntityCollection<TValue>(typeSafeDictionary.GetValues(out var count), entityids, count)
                          , exclusiveGroupStruct);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(e, "Code crashed inside Add callback ".FastConcat(fasterList[i].name));

                    throw;
                }
        }
    }
}