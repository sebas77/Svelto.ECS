using System;

namespace Svelto.ECS
{
    public static class ProcessorCount
    {
        public static readonly int   processorCount = Environment.ProcessorCount;
        
        public static int BatchSize(uint totalIterations)
        {
            var iterationsPerBatch = totalIterations / processorCount;

            if (iterationsPerBatch < 64)
                return 64;
            
            return (int) iterationsPerBatch;
        }
    }
}