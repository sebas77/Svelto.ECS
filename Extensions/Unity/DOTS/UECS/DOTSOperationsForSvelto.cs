#if UNITY_ECS
using System.Runtime.CompilerServices;
using Svelto.DataStructures;
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
#if UNITY_ECS_100
            _EManager.SetSharedComponent(e, component);
#else            
            _EManager.SetSharedComponentData(e, component);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateDOTSEntityOnSvelto(Entity prefabEntity, EGID egid)
        {
            Entity dotsEntity = _EManager.Instantiate(prefabEntity);

            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
#if UNITY_ECS_100
            _EManager.AddSharedComponent(dotsEntity, new DOTSSveltoGroupID(egid.groupID));
#else            
            _EManager.AddSharedComponentData(dotsEntity, new DOTSSveltoGroupID(egid.groupID));
#endif            
            _EManager.AddComponent<DOTSSveltoEGID>(dotsEntity);
            _EManager.SetComponentData(dotsEntity, new DOTSSveltoEGID(egid));

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
        public Entity CreateDOTSEntityOnSvelto(EntityArchetype archetype, EGID egid)
        {
            Entity dotsEntity = _EManager.CreateEntity(archetype);

            //SharedComponentData can be used to group the DOTS ECS entities exactly like the Svelto ones
#if UNITY_ECS_100            
            _EManager.AddSharedComponent(dotsEntity, new DOTSSveltoGroupID(egid.groupID));
#else
            _EManager.AddSharedComponentData(dotsEntity, new DOTSSveltoGroupID(egid.groupID));
#endif
            _EManager.AddComponent<DOTSSveltoEGID>(dotsEntity);
            _EManager.SetComponentData(dotsEntity, new DOTSSveltoEGID(egid));

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
        public Entity CreateDOTSEntity(EntityArchetype archetype)
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
        
        public T GetComponent<T>(Entity dotsEntity) where T : unmanaged, IComponentData
        {
#if UNITY_ECS_100                        
            return _EManager.GetComponentData<T>(dotsEntity);
#else
            return _EManager.GetComponentData<T>(dotsEntity);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSharedComponent<T>(Entity dotsEntity, in T component)
                where T : unmanaged, ISharedComponentData
        {
#if UNITY_ECS_100               
            _EManager.AddSharedComponent(dotsEntity, component);
#else
            _EManager.AddSharedComponentData(dotsEntity, component);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddBuffer<T>(Entity dotsEntity)
                where T : unmanaged, IBufferElementData
        {
            _EManager.AddBuffer<T>(dotsEntity);
        }
      
#if !(DEBUG && !PROFILE_SVELTO)
        [System.Diagnostics.Conditional("NO_SENSE")]    
#endif
        public void SetDebugName(Entity dotsEntity, string name)
        {
            _EManager.SetName(dotsEntity, name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSharedComponentBatched<SharedComponentData>(NativeArray<Entity> nativeArray, SharedComponentData SCD)
                where SharedComponentData : unmanaged, ISharedComponentData
        {
#if UNITY_ECS_100
            _EManager.SetSharedComponent(nativeArray, SCD);
#else            
            for (int i = 0; i < nativeArray.Length; i++)
            {
                _EManager.SetSharedComponentData(nativeArray[i], SCD);
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponentBatched<T>(NativeArray<Entity> DOTSEntities)
        {
            _EManager.AddComponent<T>(DOTSEntities);
        }

        //can't support publicly the version without DOTSSveltoEGID now
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        NativeArray<Entity> CreateDOTSEntityFromSveltoBatched(Entity prefab, (uint rangeStart, uint rangeEnd) range,
            ExclusiveGroupStruct groupID, NB<DOTSEntityComponent> DOSTEntityComponents)
        {
            unsafe
            {
                _jobHandle->Complete();

                var count = (int)(range.rangeEnd - range.rangeStart);
                var nativeArray = _EManager.Instantiate(prefab, count, _EManager.World.UpdateAllocator.ToAllocator);
                
#if UNITY_ECS_100                
                _EManager.AddSharedComponent(nativeArray, new DOTSSveltoGroupID(groupID));
#else
                for (int i = 0; i < nativeArray.Length; i++)
                {
                    _EManager.AddSharedComponentData(nativeArray[i], new DOTSSveltoGroupID(groupID));
                }
#endif

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
            ExclusiveGroupStruct groupID, NB<DOTSEntityComponent> DOSTEntityComponents, NativeEntityIDs sveltoIds, out JobHandle creationJob)
        {
            var nativeArray = CreateDOTSEntityFromSveltoBatched(prefab, range, groupID, DOSTEntityComponents);
            unsafe
            {
                var count = (int)(range.rangeEnd - range.rangeStart);
                
                _EManager.AddComponent<DOTSSveltoEGID>(nativeArray);

                var SetDOTSSveltoEGIDJob = new SetDOTSSveltoEGID
                {
                    sveltoStartIndex = range.rangeStart,
                    createdEntities = nativeArray,
                    entityManager = _EManager,
                    ids = sveltoIds,
                    groupID = groupID
                };
                creationJob = *_jobHandle = JobHandle.CombineDependencies(*_jobHandle, SetDOTSSveltoEGIDJob.ScheduleParallel(count, default));

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
        public struct SetDOTSSveltoEGID: IJobParallelFor
        {
            public uint sveltoStartIndex;
            [ReadOnly] public NativeArray<Entity> createdEntities;
            [NativeDisableParallelForRestriction] public EntityManager entityManager;
            public NativeEntityIDs ids;
            public ExclusiveGroupStruct groupID;

            public void Execute(int currentIndex)
            {
                int index = (int)(sveltoStartIndex + currentIndex);
                var dotsEntity = createdEntities[currentIndex];

                entityManager.SetComponentData(
                    dotsEntity, new DOTSSveltoEGID
                    {
                        egid = new EGID(ids[index], groupID)
                    });
            }
        }

        readonly EntityManager _EManager;
        [NativeDisableUnsafePtrRestriction] readonly unsafe JobHandle* _jobHandle;
    }
}
#endif