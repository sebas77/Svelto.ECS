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
        public static void AddEntitiesToDictionary<Strategy1, Strategy2, Strategy3, TValue>
        (in SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
       , ITypeSafeDictionary<TValue> toDic
#if SLOW_SVELTO_SUBMISSION
, in EnginesRoot.EntityReferenceMap entityLocator
#endif
       , ExclusiveGroupStruct toGroupID) where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                                         where Strategy2 : struct, IBufferStrategy<TValue>
                                         where Strategy3 : struct, IBufferStrategy<int>
                                         where TValue : struct, IBaseEntityComponent
        {
            foreach (var tuple in fromDictionary)
            {
#if SLOW_SVELTO_SUBMISSION
                    var egid = new EGID(tuple.key, toGroupID);

                    if (SlowSubmissionInfo<TValue>.hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref tuple.value, egid);

                    if (SlowSubmissionInfo<TValue>.hasReference)
                        SetEGIDWithoutBoxing<TValue>.SetRefWithoutBoxing(ref tuple.value,
                                                                         entityLocator.GetEntityReference(egid));
#endif
                try
                {
                    toDic.Add(tuple.key, tuple.value);
                }
                catch (Exception e)
                {
                    Console.LogException(
                        e, "trying to add an EntityComponent with the same ID more than once Entity: ".FastConcat(typeof(TValue).ToString()).FastConcat(", group ").FastConcat(toGroupID.ToName()).FastConcat(", id ").FastConcat(tuple.key));

                    throw;
                }
#if PARANOID_CHECK && SLOW_SVELTO_SUBMISSION
                        DBC.ECS.Check.Ensure(_hasEgid == false || ((INeedEGID)fromDictionary[egid.entityID]).ID == egid, "impossible situation happened during swap");
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesAddCallbacks<Strategy1, Strategy2, Strategy3, TValue>
        (ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
       , ITypeSafeDictionary<TValue> todic, ExclusiveGroupStruct togroup
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAdd>>> entitycomponentenginesdb
       , in PlatformProfiler sampler) where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
                                      where Strategy2 : struct, IBufferStrategy<TValue>
                                      where Strategy3 : struct, IBufferStrategy<int>
                                      where TValue : struct, IBaseEntityComponent
        {
            if (entitycomponentenginesdb.TryGetValue(new RefWrapperType(TypeCache<TValue>.type)
                                                   , out var entityComponentsEngines))
            {
                if (entityComponentsEngines.count == 0)
                    return;

                var dictionaryKeyEnumerator = fromDictionary.unsafeKeys;
                var count                   = fromDictionary.count;

                for (var i = 0; i < count; ++i)
                    try
                    {
                        var     key    = dictionaryKeyEnumerator[i].key;
                        ref var entity = ref todic.GetValueByRef(key);
                        var     egid   = new EGID(key, togroup);
                        //get all the engines linked to TValue
                        for (var j = 0; j < entityComponentsEngines.count; j++)
                            using (sampler.Sample(entityComponentsEngines[j].name))
                            {
                                ((IReactOnAdd<TValue>)entityComponentsEngines[j].engine).Add(ref entity, egid);
                            }
                    }
                    catch (Exception e)
                    {
                        Console.LogException(
                            e, "Code crashed inside Add callback with Type ".FastConcat(TypeCache<TValue>.name));

                        throw;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesDisposeCallbacks_Group<Strategy1, Strategy2, Strategy3, TValue>
        (ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnDispose>>> allEngines
       , ExclusiveGroupStruct inGroup, in PlatformProfiler sampler)
            where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
            where Strategy2 : struct, IBufferStrategy<TValue>
            where Strategy3 : struct, IBufferStrategy<int>
            where TValue : struct, IBaseEntityComponent
        {
            if (allEngines.TryGetValue(new RefWrapperType(TypeCache<TValue>.type), out var entityComponentsEngines)
             == false)
                return;

            for (var i = 0; i < entityComponentsEngines.count; i++)
                try
                {
                    using (sampler.Sample(entityComponentsEngines[i].name))
                    {
                        foreach (var value in fromDictionary)
                        {
                            ref var entity        = ref value.value;
                            var     egid          = new EGID(value.key, inGroup);
                            var     reactOnRemove = (IReactOnDispose<TValue>)entityComponentsEngines[i].engine;
                            reactOnRemove.Remove(ref entity, egid);
                        }
                    }
                }
                catch
                {
                    Console.LogError(
                        "Code crashed inside Remove callback ".FastConcat(entityComponentsEngines[i].name));

                    throw;
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesRemoveCallbacks<Strategy1, Strategy2, Strategy3, TValue>
        (FasterList<(uint, string)> infostoprocess
       , ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveenginesremove
       , ExclusiveGroupStruct fromgroup, in PlatformProfiler profiler)
            where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
            where Strategy2 : struct, IBufferStrategy<TValue>
            where Strategy3 : struct, IBufferStrategy<int>
            where TValue : struct, IBaseEntityComponent
        {
            if (reactiveenginesremove.TryGetValue(new RefWrapperType(TypeCache<TValue>.type)
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
                        var     egid   = new EGID(entityID, fromgroup);

                        for (var j = 0; j < entityComponentsEngines.count; j++)
                            using (profiler.Sample(entityComponentsEngines[j].name))
                            {
                                ((IReactOnRemove<TValue>)entityComponentsEngines[j].engine).Remove(ref entity, egid);
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
        public static void ExecuteEnginesRemoveCallbacks_Group<Strategy1, Strategy2, Strategy3, TValue>
        (ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
       , ITypeSafeDictionary<TValue> typeSafeDictionary
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemove>>> reactiveenginesremove
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnRemoveEx>>> reactiveenginesremoveex
       , int count, IEntityIDs entityids, ExclusiveGroupStruct group, in PlatformProfiler sampler)
            where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
            where Strategy2 : struct, IBufferStrategy<TValue>
            where Strategy3 : struct, IBufferStrategy<int>
            where TValue : struct, IBaseEntityComponent
        {
            if (reactiveenginesremove.TryGetValue(new RefWrapperType(TypeCache<TValue>.type)
                                                , out var reactiveEnginesRemovePerType))
            {
                var enginesCount = reactiveEnginesRemovePerType.count;

                for (var i = 0; i < enginesCount; i++)
                    try
                    {
                        foreach (var value in fromDictionary)
                        {
                            ref var entity = ref value.value;
                            var     egid   = new EGID(value.key, group);

                            using (sampler.Sample(reactiveEnginesRemovePerType[i].name))
                            {
                                ((IReactOnRemove<TValue>)reactiveEnginesRemovePerType[i].engine).Remove(
                                    ref entity, egid);
                            }
                        }
                    }
                    catch
                    {
                        Console.LogError(
                            "Code crashed inside Remove callback ".FastConcat(reactiveEnginesRemovePerType[i].name));

                        throw;
                    }
            }

            if (reactiveenginesremoveex.TryGetValue(new RefWrapperType(TypeCache<TValue>.type)
                                                  , out var reactiveEnginesRemoveExPerType))
            {
                var enginesCount = reactiveEnginesRemoveExPerType.count;

                for (var i = 0; i < enginesCount; i++)
                    try
                    {
                        using (sampler.Sample(reactiveEnginesRemoveExPerType[i].name))
                        {
                            ((IReactOnRemoveEx<TValue>)reactiveEnginesRemoveExPerType[i].engine).Remove(
                                (0, (uint)count)
                              , new EntityCollection<TValue>(typeSafeDictionary.GetValues(out _), entityids
                                                           , (uint)count), group);
                        }
                    }
                    catch
                    {
                        Console.LogError(
                            "Code crashed inside Remove callback ".FastConcat(reactiveEnginesRemoveExPerType[i].name));

                        throw;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesSwapCallbacks<Strategy1, Strategy2, Strategy3, TValue>
        (FasterList<(uint, uint, string)> infostoprocess
       , ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
       , FasterList<ReactEngineContainer<IReactOnSwap>> reactiveenginesswap, ExclusiveGroupStruct togroup
       , ExclusiveGroupStruct fromgroup, in PlatformProfiler sampler)
            where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
            where Strategy2 : struct, IBufferStrategy<TValue>
            where Strategy3 : struct, IBufferStrategy<int>
            where TValue : struct, IBaseEntityComponent
        {
            if (reactiveenginesswap.count == 0)
                return;

            var iterations = infostoprocess.count;

            for (var i = 0; i < iterations; i++)
            {
                var (fromEntityID, toEntityID, trace) = infostoprocess[i];

                try
                {
                    ref var entityComponent = ref fromDictionary.GetValueByRef(fromEntityID);
                    var     newEgid         = new EGID(toEntityID, togroup);
                    for (var j = 0; j < reactiveenginesswap.count; j++)
                        using (sampler.Sample(reactiveenginesswap[j].name))
                        {
                            ((IReactOnSwap<TValue>)reactiveenginesswap[j].engine).MovedTo(
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
        public static void ExecuteEnginesSwapCallbacks_Group<Strategy1, Strategy2, Strategy3, TValue>
        (ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
       , ITypeSafeDictionary<TValue> toDic, ExclusiveGroupStruct togroup, ExclusiveGroupStruct fromgroup
       , ITypeSafeDictionary<TValue> typeSafeDictionary
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwap>>> reactiveenginesswap
       , FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnSwapEx>>> reactiveenginesswapex
       , int count, IEntityIDs entityids, in PlatformProfiler sampler)
            where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
            where Strategy2 : struct, IBufferStrategy<TValue>
            where Strategy3 : struct, IBufferStrategy<int>
            where TValue : struct, IBaseEntityComponent
        {
            //get all the engines linked to TValue
            if (!reactiveenginesswap.TryGetValue(new RefWrapperType(TypeCache<TValue>.type)
                                               , out var reactiveEnginesSwapPerType))
                return;

            var componentsEnginesCount = reactiveEnginesSwapPerType.count;

            for (var i = 0; i < componentsEnginesCount; i++)
                try
                {
                    foreach (var value in fromDictionary)
                    {
                        ref var entityComponent = ref toDic.GetValueByRef(value.key);
                        var     newEgid         = new EGID(value.key, togroup);

                        using (sampler.Sample(reactiveEnginesSwapPerType[i].name))
                        {
                            ((IReactOnSwap<TValue>)reactiveEnginesSwapPerType[i].engine).MovedTo(
                                ref entityComponent, fromgroup, newEgid);
                        }
                    }
                }
                catch (Exception)
                {
                    Console.LogError(
                        "Code crashed inside MoveTo callback ".FastConcat(reactiveEnginesSwapPerType[i].name));

                    throw;
                }

            if (reactiveenginesswapex.TryGetValue(new RefWrapperType(TypeCache<TValue>.type)
                                                , out var reactiveEnginesRemoveExPerType))
            {
                var enginesCount = reactiveEnginesRemoveExPerType.count;

                for (var i = 0; i < enginesCount; i++)
                    try
                    {
                        using (sampler.Sample(reactiveEnginesRemoveExPerType[i].name))
                        {
                            ((IReactOnSwapEx<TValue>)reactiveEnginesRemoveExPerType[i].engine).MovedTo(
                                (0, (uint)count)
                              , new EntityCollection<TValue>(typeSafeDictionary.GetValues(out _), entityids
                                                           , (uint)count), fromgroup, togroup);
                        }
                    }
                    catch
                    {
                        Console.LogError(
                            "Code crashed inside Remove callback ".FastConcat(reactiveEnginesRemoveExPerType[i].name));

                        throw;
                    }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveEntitiesFromDictionary<Strategy1, Strategy2, Strategy3, TValue>
        (FasterList<(uint, string)> infostoprocess
       , ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
       , FasterList<uint> entityIDsAffectedByRemoval)
            where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
            where Strategy2 : struct, IBufferStrategy<TValue>
            where Strategy3 : struct, IBufferStrategy<int>
            where TValue : struct, IBaseEntityComponent
        {
            var iterations = infostoprocess.count;

            for (var i = 0; i < iterations; i++)
            {
                var (id, trace) = infostoprocess[i];

                try
                {
                    if (fromDictionary.Remove(id, out var index, out var value))
                    {
                        //Note I am doing this to be able to use a range of values even with the 
                        //remove Ex callbacks. Basically I am copying back the deleted value
                        //at the end of the array, so I can use as range count, count + number of deleted entities
                        //I need to swap the keys too to have matching EntityIDs
                        fromDictionary.unsafeValues[(uint)fromDictionary.count] = value;
                        fromDictionary.unsafeKeys[(uint)fromDictionary.count] = new SveltoDictionaryNode<uint>(ref id, 0);
                        //when a component is removed from a component array, a remove swap back happens. This means
                        //that not only we have to remove the index of the component of the entity deleted from the array
                        //but we need also to update the index of the component that has been swapped in the cell
                        //of the deleted component 
                        //entityIDsAffectedByRemoval tracks all the entitiesID of the components that need to be updated
                        //in the filters because their indices in the array changed. 
                        entityIDsAffectedByRemoval.Add(fromDictionary.unsafeKeys[index].key);
                    }
                }
                catch
                {
                    var str = "Crash while executing Remove Entity operation on ".FastConcat(TypeCache<TValue>.name)
                       .FastConcat(" from : ", trace);

                    Console.LogError(str);

                    throw;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SwapEntitiesBetweenDictionaries<Strategy1, Strategy2, Strategy3, TValue>
        (FasterList<(uint, uint, string)> infostoprocess
       , ref SveltoDictionary<uint, TValue, Strategy1, Strategy2, Strategy3> fromDictionary
       , ITypeSafeDictionary<TValue> toDictionary, ExclusiveGroupStruct fromgroup, ExclusiveGroupStruct togroup
       , FasterList<uint> entityIDsAffectedByRemoval)
            where Strategy1 : struct, IBufferStrategy<SveltoDictionaryNode<uint>>
            where Strategy2 : struct, IBufferStrategy<TValue>
            where Strategy3 : struct, IBufferStrategy<int>
            where TValue : struct, IBaseEntityComponent
        {
            var iterations = infostoprocess.count;

            for (var i = 0; i < iterations; i++)
            {
                var (fromID, toID, trace) = infostoprocess[i];

                try
                {
                    var fromEntityGid = new EGID(fromID, fromgroup);
                    var toEntityEgid  = new EGID(toID, togroup);

                    Check.Require(togroup.isInvalid == false, "Invalid To Group");

                    if (fromDictionary.Remove(fromEntityGid.entityID, out var index, out var value))
                        entityIDsAffectedByRemoval.Add(fromDictionary.unsafeKeys[index].key);
                    else
                        Check.Assert(false, "Swapping an entity that doesn't exist");

#if SLOW_SVELTO_SUBMISSION
                    if (SlowSubmissionInfo<TValue>.hasEgid)
                        SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref value, toEntityEgid);
#endif

                    toDictionary.Add(toEntityEgid.entityID, value);

#if PARANOID_CHECK
                        DBC.ECS.Check.Ensure(_hasEgid == false || ((INeedEGID)toGroupCasted[toEntityEGID.entityID]).ID == toEntityEGID, "impossible situation happened during swap");
#endif
                }
                catch
                {
                    var str = "Crash while executing Swap Entity operation on ".FastConcat(TypeCache<TValue>.name)
                       .FastConcat(" from : ", trace);

                    Console.LogError(str);

                    throw;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesAddEntityCallbacksFast<TValue>
        (FasterDictionary<RefWrapperType, FasterList<ReactEngineContainer<IReactOnAddEx>>> fasterDictionary
       , ExclusiveGroupStruct groupId, (uint, uint) valueTuple, IEntityIDs entityids
       , ITypeSafeDictionary<TValue> typeSafeDictionary, PlatformProfiler profiler)
            where TValue : struct, IBaseEntityComponent
        {
            //get all the engines linked to TValue
            if (!fasterDictionary.TryGetValue(new RefWrapperType(TypeCache<TValue>.type)
                                            , out var entityComponentsEngines))
                return;

            for (var i = 0; i < entityComponentsEngines.count; i++)
                try
                {
                    using (profiler.Sample(entityComponentsEngines[i].name))
                    {
                        ((IReactOnAddEx<TValue>)entityComponentsEngines[i].engine).Add(
                            valueTuple
                          , new EntityCollection<TValue>(typeSafeDictionary.GetValues(out var count), entityids, count)
                          , groupId);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(
                        e, "Code crashed inside Add callback ".FastConcat(entityComponentsEngines[i].name));

                    throw;
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesSwapCallbacksFast<TValue>
        (FasterList<ReactEngineContainer<IReactOnSwapEx>> fasterList, ExclusiveGroupStruct fromGroup
       , ExclusiveGroupStruct toGroup, IEntityIDs entityids, ITypeSafeDictionary<TValue> typeSafeDictionary
       , (uint, uint) rangeofsubmittedentitiesindicies, PlatformProfiler sampler)
            where TValue : struct, IBaseEntityComponent
        {
            for (var i = 0; i < fasterList.count; i++)
                try
                {
                    using (sampler.Sample(fasterList[i].name))
                    {
                        ((IReactOnSwapEx<TValue>)fasterList[i].engine).MovedTo(rangeofsubmittedentitiesindicies
                                                                             , new EntityCollection<TValue>(
                                                                                   typeSafeDictionary.GetValues(
                                                                                       out var count), entityids, count)
                                                                             , fromGroup, toGroup);
                    }
                }
                catch (Exception e)
                {
                    Console.LogException(e, "Code crashed inside Add callback ".FastConcat(fasterList[i].name));

                    throw;
                }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ExecuteEnginesRemoveCallbacksFast<TValue>
        (FasterList<ReactEngineContainer<IReactOnRemoveEx>> fasterList, ExclusiveGroupStruct exclusiveGroupStruct
       , (uint, uint) valueTuple, IEntityIDs entityids, ITypeSafeDictionary<TValue> typeSafeDictionary
       , PlatformProfiler sampler) where TValue : struct, IBaseEntityComponent
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