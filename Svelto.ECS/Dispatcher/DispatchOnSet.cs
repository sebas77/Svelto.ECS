using System;
using Svelto.WeakEvents;

namespace Svelto.ECS
{
    public class DispatchOnSet<T> where T:struct
    {
        public DispatchOnSet(EGID senderID)
        {      
            _subscribers = new WeakEvent<EGID, T>();
            
            _senderID = senderID;
        }
        
        public T value
        {
            set
            {
                _value = value;

                if(_paused == false)
                    _subscribers.Invoke(_senderID, value);
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

        public void PauseNotify() { _paused = true; }
        public void ResumeNotify() { _paused = false; }

        protected T  _value;
        readonly EGID _senderID;

        WeakEvent<EGID, T> _subscribers;
        bool _paused;
    }
}
