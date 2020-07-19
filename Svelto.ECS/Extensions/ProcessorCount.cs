using System;

namespace Svelto.ECS
{
    internal static class ProcessorCount
    {
        static readonly int processorCount = Environment.ProcessorCount;

        public static int BatchSize(uint totalIterations)
        {
            var iterationsPerBatch = totalIterations / processorCount;

            if (iterationsPerBatch < 32)
                return 32;
            
            return (int) iterationsPerBatch;
        }
    }
}