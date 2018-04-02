using System.Collections;

#if ENGINE_PROFILER_ENABLED && UNITY_EDITOR
using Svelto.ECS.Profiler;
#endif

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        class DoubleBufferedEntityViews<T> where T : class, IDictionary, new()
        {
            readonly T _entityViewsToAddBufferA = new T();
            readonly T _entityViewsToAddBufferB = new T();

            internal DoubleBufferedEntityViews()
            {
                this.other = _entityViewsToAddBufferA;
                this.current = _entityViewsToAddBufferB;
            }

            internal T other;
            internal T current;

            internal void Swap()
            {
                var toSwap = other;
                other = current;
                current = toSwap;
            }
        }
    }
}