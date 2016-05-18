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

                Dispatch(ref value);
            }
        }
    }

    T _value;
}

public class DispatcherOnSet<S, T>: Dispatcher<S, T>
{
    public DispatcherOnSet(S sender) : base(sender) { }

    public T value
    {
        set
        {
            Dispatch(ref value);
        }
    }
}