namespace Svelto.ECS.Internal
{
    public interface IHandleEntityViewEngineAbstracted : IEngine
    {}
    
    public interface IHandleEntityStructEngine<T> : IHandleEntityViewEngineAbstracted
    {
        void AddInternal(ref    T entityView);
        void RemoveInternal(ref T entityView);
    }
    
    public class EngineInfo
    {
#if ENABLE_PLATFORM_PROFILER
        public EngineInfo()
        {
            name = GetType().FullName;
        }
#endif  
        internal string name;
    }
}

namespace Svelto.ECS
{
    public interface IEngine
    {}
}