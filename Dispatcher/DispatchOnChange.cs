using System;

namespace Svelto.ECS
{
    public class DispatchOnChange<T> : DispatchOnSet<T> where T:IEquatable<T>
    {
        public DispatchOnChange(EGID senderID, T initialValue = default(T)) : base(senderID)
        {
            _value = initialValue;
        }
        
        public new T value
        {
            set
            {
                if (value.Equals(_value) == false)
                    base.value = value;
            }

            get => _value;
        }
    }
}
