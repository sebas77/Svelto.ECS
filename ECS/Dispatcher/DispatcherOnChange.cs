using System.Collections.Generic;

namespace Svelto.ECS
{
    public class DispatchOnChange<T> : DispatchOnSet<T>
    {
        public DispatchOnChange(int senderID) : base(senderID)
        { }

        public new T value
        {
            set
            {
                if (EqualityComparer<T>.Default.Equals(value, _value) == false)
                    base.value = value;
            }

            get 
            {
                return _value;
            }
        }
    }
}
