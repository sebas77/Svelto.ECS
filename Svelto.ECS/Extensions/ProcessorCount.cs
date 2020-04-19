using System;

namespace Svelto.ECS
{
    internal static class ProcessorCount
    {
        static readonly int processorCount = Environment.ProcessorCount;

        public static int BatchSize(uint totalIterations)
        {
            var iterationsPerBatch = totalIterations / processorCount;

            if (iterationsPerBatch < 16)
                return 16;
            
            return (int) iterationsPerBatch;
        }
    }
}