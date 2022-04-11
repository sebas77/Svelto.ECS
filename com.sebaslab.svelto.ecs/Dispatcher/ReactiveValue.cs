using System;
using System.Collections.Generic;

namespace Svelto.ECS
{
    /// <summary>
    /// Reasons why unfortunately this cannot be a struct:
    /// the user must remember to create interface with ref getters
    /// ref getters cannot have set, while we sometimes use set to initialise values
    /// the struct will be valid even if it has not ever been initialised
    ///
    /// 1 and 3 are possibly solvable, but 2 is a problem
    /// </summary>
    /// <typeparam name="T"></typeparam>{
    public class ReactiveValue<T>
    {
        public ReactiveValue
        (EntityReference senderID, Action<EntityReference, T> callback, T initialValue = default
       , bool notifyImmediately = false, ReactiveType notifyOnChange = ReactiveType.ReactOnChange)
        {
            _subscriber = callback;

            if (notifyImmediately)
                _subscriber(senderID, initialValue);

            _senderID       = senderID;
            _value          = initialValue;
            _notifyOnChange = notifyOnChange;
        }

        public ReactiveValue(EntityReference senderID, Action<EntityReference, T> callback, ReactiveType notifyOnChange)
        {
            _subscriber     = callback;
            _notifyOnChange = notifyOnChange;
            _senderID       = senderID;
        }

        public T value
        {
            set
            {
                if (_notifyOnChange == ReactiveType.ReactOnSet || _comp.Equals(_value, value) == false)
                {
                    if (_paused == false)
                        _subscriber(_senderID, value);

                    //all the subscribers relies on the actual value not being changed yet, as the second parameter
                    //is the new value
                    _value = value;
                }
            }
            get => _value;
        }

        public void PauseNotify()
        {
            _paused = true;
        }

        public void ResumeNotify()
        {
            _paused = false;
        }
        
        public void ForceValue(in T value)
        {
            if (_paused == false)
                _subscriber(_senderID, value);

            _value = value;
        }

        public void SetValueWithoutNotify(in T value)
        {
            _value = value;
        }

        public void StopNotify()
        {
            _subscriber = null;
            _paused     = true;
        }

        readonly ReactiveType        _notifyOnChange;
        readonly EntityReference     _senderID;
        bool                         _paused;
        Action<EntityReference, T>   _subscriber;
        T                            _value;
        static readonly EqualityComparer<T> _comp = EqualityComparer<T>.Default;
    }

    public enum ReactiveType
    {
        ReactOnSet,
        ReactOnChange
    }
}