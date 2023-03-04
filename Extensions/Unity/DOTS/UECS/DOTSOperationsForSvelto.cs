#if UNITY_ECS
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    public readonly struct DOTSOperationsForSvelto
    {
        internal unsafe DOTSOperationsForSvelto(EntityManager manager, JobHandle* jobHandle)
        {
            _EManager = manager;
            _jobHandle = jobHandle;
        }

        public EntityArchetype CreateArchetype(params ComponentType[] types)
        {
            return _EManager.CreateArchetype(types);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetComponent<T>(Entity e, in T component)
                where T : unmanaged, IComponentData
        {
            _EManager.SetComponentData(e, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSharedComponent<T>(Entity e, in T component)
                where T : unmanaged, ISharedComponentData
        {
            _EManager.SetSharedComponent(e, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Entity CreateDOTSEntityFromSvelto(Entity prefabEntity, ExclusiveGroupStruct groupID, EntityReference reference)
        {
            Entity dotsEntity = _EManager.Instantiate(prefabEntity);

            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
            _EManager.AddSharedComponent(dotsEntity, new DOTSSveltoGroupID(groupID));
            _EManager.AddComponent<DOTSSveltoReference>(dotsEntity);
            _EManager.SetComponentData(dotsEntity, new DOTSSveltoReference(reference));

            return dotsEntity;
        }

        /// <summary>
        /// This method assumes that the Svelto entity with EGID egid has also dotsEntityComponent
        /// among the descriptors
        /// </summary>
        /// <param name="archetype"></param>
        /// <param name="egid"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Entity CreateDOTSEntityFromSvelto(EntityArchetype archetype, ExclusiveGroupStruct groupID, EntityReference reference)
        {
            Entity dotsEntity = _EManager.CreateEntity(archetype);

            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
            _EManager.AddSharedComponent(dotsEntity, new DOTSSveltoGroupID(groupID));
            _EManager.AddComponent<DOTSSveltoReference>(dotsEntity);
            _EManager.SetComponentData(dotsEntity, new DOTSSveltoReference(reference));

            return dotsEntity;
        }

        /// <summary>
        /// in this case the user decided to create a DOTS entity that is self managed and not managed
        /// by the framework
        /// </summary>
        /// <param name="archetype"></param>
        /// <param name="wireEgid"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Entity CreateDOTSEntity(EntityArchetype archetype)
        {
            return _EManager.CreateEntity(archetype);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntity(Entity e)
        {
            _EManager.DestroyEntity(e);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent<T>(Entity dotsEntity)
        {
            _EManager.RemoveComponent<T>(dotsEntity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(Entity dotsEntity)
                where T : unmanaged, IComponentData
        {
            _EManager.AddComponent<T>(dotsEntity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(Entity dotsEntity, in T component)
                where T : unmanaged, IComponentData
        {
            _EManager.AddComponent<T>(dotsEntity);
            _EManager.SetComponentData(dotsEntity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSharedComponent<T>(Entity dotsEntity, in T component)
                where T : unmanaged, ISharedComponentData
        {
            _EManager.AddSharedComponent(dotsEntity, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBuffer<T>(Entity dotsEntity)
                where T : unmanaged, IBufferElementData
        {
            _EManager.AddBuffer<T>(dotsEntity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSharedComponentBatched<SharedComponentData>(NativeArray<Entity> nativeArray, SharedComponentData SCD)
                where SharedComponentData : unmanaged, ISharedComponentData
        {
            _EManager.SetSharedComponent(nativeArray, SCD);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponentBatched<T>(NativeArray<Entity> DOTSEntities)
        {
            _EManager.AddComponent<T>(DOTSEntities);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<Entity> CreateDOTSEntityFromSveltoBatched(Entity prefab, (uint rangeStart, uint rangeEnd) range,
            ExclusiveGroupStruct groupID, NB<DOTSEntityComponent> DOSTEntityComponents)
        {
            unsafe
            {
                _jobHandle->Complete();

                var count = (int)(range.rangeEnd - range.rangeStart);
                var nativeArray = _EManager.Instantiate(prefab, count, _EManager.World.UpdateAllocator.ToAllocator);
                _EManager.AddSharedComponent(nativeArray, new DOTSSveltoGroupID(groupID));

                var setDOTSEntityComponentsJob = new SetDOTSEntityComponents
                {
                    sveltoStartIndex = range.rangeStart,
                    createdEntities = nativeArray,
                    DOSTEntityComponents = DOSTEntityComponents
                };
                *_jobHandle = JobHandle.CombineDependencies(*_jobHandle, setDOTSEntityComponentsJob.ScheduleParallel(count, default));

                return nativeArray;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<Entity> CreateDOTSEntityFromSveltoBatched(Entity prefab, (uint rangeStart, uint rangeEnd) range,
            ExclusiveGroupStruct groupID, NB<DOTSEntityComponent> DOSTEntityComponents,
            SharedSveltoDictionaryNative<uint, EntityReference> referenceMap, NativeEntityIDs sveltoIds, out JobHandle creationJob)
        {
            var nativeArray = CreateDOTSEntityFromSveltoBatched(prefab, range, groupID, DOSTEntityComponents);
            unsafe
            {
                var count = (int)(range.rangeEnd - range.rangeStart);
                
                _EManager.AddComponent<DOTSSveltoReference>(nativeArray);

                var SetDOTSSveltoReferenceJob = new SetDOTSSveltoReference
                {
                    sveltoStartIndex = range.rangeStart,
                    createdEntities = nativeArray,
                    entityManager = _EManager,
                    ids = sveltoIds,
                    entityReferenceMap = referenceMap,
                };
                creationJob = *_jobHandle = JobHandle.CombineDependencies(*_jobHandle, SetDOTSSveltoReferenceJob.ScheduleParallel(count, default));

                return nativeArray;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddJobToComplete(JobHandle jobHandle)
        {
            unsafe
            {
                *_jobHandle = JobHandle.CombineDependencies(*_jobHandle, jobHandle);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntitiesBatched(NativeArray<Entity> nativeArray)
        {
            _EManager.DestroyEntity(nativeArray);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Complete()
        {
            unsafe
            {
                _jobHandle->Complete();
            }
        }

        [BurstCompile]
        struct SetDOTSEntityComponents: IJobParallelFor
        {
            public uint sveltoStartIndex;
            [ReadOnly] public NativeArray<Entity> createdEntities;
            public NB<DOTSEntityComponent> DOSTEntityComponents;

            public void Execute(int currentIndex)
            {
                int index = (int)(sveltoStartIndex + currentIndex);
                var dotsEntity = createdEntities[currentIndex];

                DOSTEntityComponents[index].dotsEntity = dotsEntity;
            }
        }

        [BurstCompile]
        public struct SetDOTSSveltoReference: IJobParallelFor
        {
            public uint sveltoStartIndex;
            [ReadOnly] public NativeArray<Entity> createdEntities;
            [NativeDisableParallelForRestriction] public EntityManager entityManager;
            public NativeEntityIDs ids;
            public SharedSveltoDictionaryNative<uint, EntityReference> entityReferenceMap;

            public void Execute(int currentIndex)
            {
                int index = (int)(sveltoStartIndex + currentIndex);
                var dotsEntity = createdEntities[currentIndex];

                entityManager.SetComponentData(
                    dotsEntity, new DOTSSveltoReference
                    {
                        entityReference = entityReferenceMap[ids[index]]
                    });
            }
        }

        readonly EntityManager _EManager;
        [NativeDisableUnsafePtrRestriction] readonly unsafe JobHandle* _jobHandle;
    }
}
#endif