using System;

namespace Svelto.Observer
{
    public interface IObservable<DispatchType>
    { 
        event Action<DispatchType> Notify;

        void Dispatch(DispatchType parameter);
    }

    public class Observable<DispatchType>:IObservable<DispatchType>
    {
        public event Action<DispatchType> Notify;

        public void Dispatch(DispatchType parameter)
        {
            Notify(parameter);
        }
    }
}
