#if UNITY_JOBS
using System;
using Svelto.ECS.SveltoOnDOTS;
using Unity.Jobs;

//note can't change namespace, too late for old projects
namespace Svelto.ECS
{
    public static class UnityJobExtensions
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
            <JOB>(this JOB job, int iterations, JobHandle inputDeps) where JOB: struct, IJobParallelFor
        {
            if (iterations <= 0)
                return inputDeps;

            var innerloopBatchCount = ProcessorCount.BatchSize((uint) iterations);
            return job.Schedule((int)iterations, innerloopBatchCount, inputDeps);
        }

        public static JobHandle ScheduleParallel
            <JOB>(this JOB job, uint iterations, JobHandle inputDeps) where JOB: struct, IJobParallelFor
        {
            if (iterations == 0)
                return inputDeps;

            var innerloopBatchCount = ProcessorCount.BatchSize(iterations);
            return job.Schedule((int)iterations, innerloopBatchCount, inputDeps);
        }

        public static JobHandle ScheduleParallelAndCombine
            <JOB>(this JOB job, int iterations, JobHandle inputDeps, JobHandle combinedDeps) where JOB: struct, IJobParallelFor
        {
            if (iterations == 0)
                return combinedDeps;

            var innerloopBatchCount = ProcessorCount.BatchSize((uint)iterations);
            var jobDeps = job.Schedule(iterations, innerloopBatchCount, inputDeps);

            return JobHandle.CombineDependencies(combinedDeps, jobDeps);
        }

        public static JobHandle ScheduleAndCombine
            <JOB>(this JOB job, JobHandle inputDeps, JobHandle combinedDeps) where JOB : struct, IJob
        {
            var jobDeps = job.Schedule(inputDeps);
            return JobHandle.CombineDependencies(combinedDeps, jobDeps);
        }

        public static JobHandle ScheduleAndCombine
            <JOB>(this JOB job, int arrayLength, JobHandle inputDeps, JobHandle combinedDeps) where JOB : struct, IJobFor
        {
            if (arrayLength == 0)
                return combinedDeps;
            
            var jobDeps = job.Schedule(arrayLength, inputDeps);
            return JobHandle.CombineDependencies(combinedDeps, jobDeps);
        }
    }
}
#endif