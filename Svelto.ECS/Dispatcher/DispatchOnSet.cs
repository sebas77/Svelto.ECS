using System;
using Svelto.WeakEvents;

namespace Svelto.ECS
{
    public class DispatchOnSet<T> where T:struct
    {
        static ExclusiveGroup OBSOLETE_GROUP = new ExclusiveGroup();
        
        public DispatchOnSet(int senderID)
        {
            Console.LogWarningDebug("This method is obsolete and shouldn't be used anymore");
            
            _senderID    = new EGID(senderID, OBSOLETE_GROUP);
            _subscribers = new WeakEvent<EGID, T>();
        }

        public DispatchOnSet(EGID senderID)
        {      
            _subscribers = new WeakEvent<EGID, T>();
            
            _senderID = senderID;
        }
        
        public DispatchOnSet()
        {      
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
        
        public void NotifyOnValueSet(Action<EGID, T> action)
        {
            _subscribers += action;
        }

        public void StopNotify(Action<EGID, T> action)
        {
            _subscribers -= action;
        }

        protected T  _value;
        readonly EGID _senderID;

        WeakEvent<EGID, T> _subscribers;
    }
}
