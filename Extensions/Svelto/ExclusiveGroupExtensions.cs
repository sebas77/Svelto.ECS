using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    //TODO there is an overlap between these methods and Group Compound Includes
    public static class ExclusiveGroupExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool FoundIn(this in ExclusiveGroupStruct group, ExclusiveGroupStruct[] groups)
        {
            for (int i = 0; i < groups.Length; ++i)
                if (groups[i] == group)
                    return true;

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool FoundIn(this in ExclusiveGroupStruct group, FasterList<ExclusiveGroupStruct> groups)
        {
            for (int i = 0; i < groups.count; ++i)
                if (groups[i] == group)
                    return true;

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool FoundIn(this in ExclusiveGroupStruct group, LocalFasterReadOnlyList<ExclusiveGroupStruct> groups)
        {
            for (int i = 0; i < groups.count; ++i)
                if (groups[i] == group)
                    return true;

            return false;
        }
    }
}