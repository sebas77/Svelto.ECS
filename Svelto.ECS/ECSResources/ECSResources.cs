using Svelto.DataStructures;

namespace Svelto.ECS.Experimental
{
    public struct ECSResources<T>
    {
        internal uint id;
        
        public static implicit operator T(ECSResources<T> ecsString) { return ResourcesECSDB<T>.FromECS(ecsString.id); }
    }
    
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

        internal static uint ToECS(T resource)
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

    public static class ResourceExtensions
    {
        public static void Set<T>(ref this ECSResources<T> resource, T newText)
        {
            if (resource.id != 0)
                ResourcesECSDB<T>.resources(resource.id) = newText;
            else
                resource.id = ResourcesECSDB<T>.ToECS(newText);
        }
    }
}