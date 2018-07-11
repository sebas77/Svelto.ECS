using Svelto.WeakEvents;

namespace Svelto.ECS
{
    public class DispatchOnSet<T> where T:struct
    {
        public DispatchOnSet(int senderID)
        {
            _senderID    = senderID;
            _subscribers = new WeakEvent<int, T>();
        }

        public DispatchOnSet()
        {      
            _subscribers = new WeakEvent<int, T>();
        }
        
        public T value
        {
            set
            {
                _value = value;

                _subscribers.Invoke(_senderID, value);
            }

            get 
            {
                return _value;
            }
        }
        
        public void NotifyOnValueSet(System.Action<int, T> action)
        {
            _subscribers += action;
        }

        public void StopNotify(System.Action<int, T> action)
        {
            _subscribers -= action;
        }

        protected T      _value;
        readonly int _senderID;

        WeakEvent<int, T> _subscribers;
    }
}
