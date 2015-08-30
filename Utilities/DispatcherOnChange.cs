using System.Collections.Generic;

public class DispatcherOnChange<S, T>: Dispatcher<S, T>
{
    public DispatcherOnChange(S sender) : base(sender) { }

    public T value
    {
        set
        {
            if (EqualityComparer<T>.Default.Equals(value, _value) == false)
            {
                _value = value;

                Dispatch(value);
            }
        }
    }

    T _value;
}

