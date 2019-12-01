using System;

namespace Svelto.ServiceLayer.Experimental
{
    public interface IServiceEventContainer : IDisposable
    {
        //Delegate constraints to store delegates without needing a signature
        void ListenTo<TListener, TDelegate>(TDelegate callBack)
            where TListener : class, IServiceEventListener<TDelegate> where TDelegate : Delegate;
    }
}
