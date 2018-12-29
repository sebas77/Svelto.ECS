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
        protected EngineInfo()
        {
            name = GetType().FullName;
        }
#endif    
        internal readonly string name = string.Empty;
    }
}

namespace Svelto.ECS
{
    public interface IEngine
    {}
}