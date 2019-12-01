using System;

namespace Svelto.ServiceLayer.Experimental
{
    public interface IServiceEventListener<in TDelegate> : IServiceEventListenerBase where TDelegate : Delegate
    {
        void SetCallback(TDelegate callback);
    }

    // This interface exists so we can use one type which can represent any of the interfaces above
    public interface IServiceEventListenerBase : IDisposable
    {
    }
}
