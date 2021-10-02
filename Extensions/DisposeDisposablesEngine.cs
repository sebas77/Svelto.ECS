using System;

namespace Svelto.ECS
{
    [AllowMultiple]
    public class DisposeDisposablesEngine : IEngine, IDisposable
    {
        public DisposeDisposablesEngine(IDisposable[] disposable)
        {
            _disposable = disposable;
        }
        
        public void Dispose()
        {
            foreach (var d in _disposable)
            {
                d.Dispose();
            }
        }

        IDisposable[] _disposable;
    }

}