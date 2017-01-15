using System.Collections.Generic;

namespace Svelto.ECS
{
    public class DispatcherOnChange<T> : DispatcherOnSet<T>
    {
        public DispatcherOnChange(int senderID) : base(senderID)
        { }

        public new T value
        {
            set
            {
                if (EqualityComparer<T>.Default.Equals(value, _value) == false)
                    base.value = _value;
            }

            get 
            {
                return _value;
            }
        }
    }
}
