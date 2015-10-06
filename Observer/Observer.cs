using System;

namespace Svelto.Observer
{
    public abstract class Observer<DispatchType, ActionType>: IObserver<ActionType>
    {
        public Observer(Observable<DispatchType> observable)
        {
             observable.Notify += OnObservableDispatched;

            _unsubscribe = () => observable.Notify -= OnObservableDispatched;
        }

        public void AddAction(Action<ActionType> action)
        {
            _actions += action;
        }

        public void RemoveAction(Action<ActionType> action)
        {
            _actions += action;
        }

        public void Unsubscribe()
        {
            _unsubscribe();
        }

        private void OnObservableDispatched(DispatchType dispatchNotification)
        {
             _actions(TypeMap(dispatchNotification));
        }

        abstract protected ActionType TypeMap(DispatchType dispatchNotification);

        Action<ActionType>  _actions;
        Action              _unsubscribe;
    }

    public class Observer<DispatchType>: Observer<DispatchType, DispatchType>
    {
        public Observer(Observable<DispatchType> observable):base(observable)
        {}

        protected override DispatchType TypeMap(DispatchType dispatchNotification)
        {
            return dispatchNotification;
        }
    }

    public interface IObserver<WatchingType>
    {
        void AddAction(Action<WatchingType> action);
        void RemoveAction(Action<WatchingType> action);

        void Unsubscribe();
    }
}
