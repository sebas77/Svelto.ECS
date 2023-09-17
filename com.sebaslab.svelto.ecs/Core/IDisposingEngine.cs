using System;

namespace Svelto.ECS
{
    public interface IDisposableEngine: IDisposable
    {
        bool isDisposing { set; }
    }
}