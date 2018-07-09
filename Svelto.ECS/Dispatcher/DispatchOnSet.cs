using Svelto.WeakEvents;

namespace Svelto.ECS
{
    public class DispatchOnSet<T> where T:struct
    {
        public DispatchOnSet(int senderID):this()
        {
            _senderID    = new EGID(senderID, ExclusiveGroup.StandardEntitiesGroup);
        }
        
        public DispatchOnSet(EGID senderID):this()
        {
            _senderID = senderID;
        }
        
        public DispatchOnSet()
        {
            _senderID    = new EGID();
            _subscribers = new WeakEvent<EGID, T>();
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
        
        public void NotifyOnValueSet(System.Action<EGID, T> action)
        {
            _subscribers += action;
        }

        public void StopNotify(System.Action<EGID, T> action)
        {
            _subscribers -= action;
        }

        protected T      _value;
        readonly EGID _senderID;

        WeakEvent<EGID, T> _subscribers;
    }
}
