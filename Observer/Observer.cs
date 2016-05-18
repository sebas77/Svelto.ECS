using System;

namespace Svelto.Observer.InterNamespace
{
    public abstract class Observer<DispatchType, ActionType> : IObserver<ActionType>
    {
        protected Observer(Observable<DispatchType> observable)
        {
            observable.Notify += OnObservableDispatched;

            _unsubscribe = () => observable.Notify -= OnObservableDispatched;
        }

        public void AddAction(ObserverAction<ActionType> action)
        {
            _actions += action;
        }

        public void RemoveAction(ObserverAction<ActionType> action)
        {
            _actions -= action;
        }

        public void Unsubscribe()
        {
            _unsubscribe();
        }

        void OnObservableDispatched(ref DispatchType dispatchNotification)
        {
            if (_actions != null)
            {
                var actionType = TypeMap(ref dispatchNotification);

                _actions(ref actionType);
            }
        }

        protected abstract ActionType TypeMap(ref DispatchType dispatchNotification);

        ObserverAction<ActionType> _actions;
        Action _unsubscribe;
    }
}

namespace Svelto.Observer.IntraNamespace
{
    public class Observer<DispatchType> : InterNamespace.Observer<DispatchType, DispatchType>
    {
        public Observer(Observable<DispatchType> observable) : base(observable)
        { }

        protected override DispatchType TypeMap(ref DispatchType dispatchNotification)
        {
            return dispatchNotification;
        }
    }
}

namespace Svelto.Observer
{
    public class Observer: IObserver
    {
        public Observer(Observable observable)
        {
             observable.Notify += OnObservableDispatched;

            _unsubscribe = () => observable.Notify -= OnObservableDispatched;
        }

        public void AddAction(Action action)
        {
            _actions += action;
        }

        public void RemoveAction(Action action)
        {
            _actions -= action;
        }

        public void Unsubscribe()
        {
            _unsubscribe();
        }

        void OnObservableDispatched()
        {
            if (_actions != null)
             _actions();
        }

        Action  _actions;
        readonly Action  _unsubscribe;
    }

    public interface IObserver<WatchingType>
    {
        void AddAction(ObserverAction<WatchingType> action);
        void RemoveAction(ObserverAction<WatchingType> action);

        void Unsubscribe();
    }

    public interface IObserver
    {
        void AddAction(Action action);
        void RemoveAction(Action action);

        void Unsubscribe();
    }
}