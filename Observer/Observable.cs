using System;

namespace Svelto.Observer
{
    public delegate void ObserverAction<DispatchType>(ref DispatchType parameter);

    public interface IObservable
    { 
        event Action Notify;

        void Dispatch();
    }

    public interface IObservable<DispatchType>
    { 
        event ObserverAction<DispatchType> Notify;

        void Dispatch(ref DispatchType parameter);
    }

    public class Observable<DispatchType>:IObservable<DispatchType>
    {
        public event ObserverAction<DispatchType> Notify;

        public void Dispatch(ref DispatchType parameter)
        {
            if (Notify != null)
                Notify(ref parameter);
        }
    }

    public class Observable:IObservable
    {
        public event Action Notify;

        public void Dispatch()
        {
            if (Notify != null)
                Notify();
        }
    }
}
