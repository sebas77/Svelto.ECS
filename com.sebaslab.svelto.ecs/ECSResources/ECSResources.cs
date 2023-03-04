using Svelto.DataStructures;

namespace Svelto.ECS.ResourceManager
{
    /// <summary>
    /// To do. Or we reuse the ID or we need to clear this
    /// </summary>
    /// <typeparam name="T"></typeparam>
    static class ResourcesECSDB<T>
    {
        static readonly FasterList<T> _resources = new FasterList<T>();

        internal static ref T resources(uint id)
        {
            return ref _resources[(int) id - 1];
        }

        internal static uint ToECS(in T resource)
        {
            _resources.Add(resource);

            return (uint)_resources.count;
        }

        public static T FromECS(uint id)
        {
            if (id - 1 < _resources.count)
                return _resources[(int) id - 1];
            
            return default;
        }
    }   
}