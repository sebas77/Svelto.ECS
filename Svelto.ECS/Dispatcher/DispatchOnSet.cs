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

                _subscribers.Invoke(_senderID, value);
            }

            get => _value;
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
        internal EGID _senderID;

        WeakEvent<EGID, T> _subscribers;
    }

    public static class DispatchExtensions
    {
        public static DispatchOnSet<T> Setup<T>(DispatchOnSet<T> dispatcher, EGID entity) where T : struct
        {
            if (dispatcher == null)
                dispatcher = new DispatchOnSet<T>(entity);
            else
                dispatcher._senderID = entity;

            return dispatcher;
        }
        
        public static DispatchOnChange<T> Setup<T>(DispatchOnChange<T> dispatcher, EGID entity)
            where T : struct, IEquatable<T>
        {
            if (dispatcher == null)
                dispatcher = new DispatchOnChange<T>(entity);
            else
                dispatcher._senderID = entity;
            
            return dispatcher;
        }
    }
}
