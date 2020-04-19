#if UNITY_2019_2_OR_NEWER
using System;
using Svelto.Common;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public static class UnityEntityDBExtensions
    {
        public static JobHandle ScheduleDispose
            <T1>(this T1 disposable, JobHandle inputDeps) where T1 : struct, IDisposable
        {
            return new DisposeJob<T1>(disposable).Schedule(inputDeps);
        }
        
        public static JobHandle ScheduleDispose
            <T1, T2>(this T1 disposable1, T2 disposable2, JobHandle inputDeps) 
                where T1 : struct, IDisposable where T2 : struct, IDisposable
        {
            return new DisposeJob<T1, T2>(disposable1, disposable2).Schedule(inputDeps);
        }
        
        public static JobHandle ScheduleParallel
            <JOB>(this JOB job, uint iterations, JobHandle inputDeps) where JOB: struct, IJobParallelFor
        {
            var innerloopBatchCount = ProcessorCount.BatchSize(iterations);
            return job.Schedule((int)iterations, innerloopBatchCount, inputDeps);
        }
    }
}
#endif