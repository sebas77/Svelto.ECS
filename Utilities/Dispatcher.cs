public class Dispatcher<S, T>
{
    public event System.Action<S, T> observers;

    private Dispatcher() { }

    public Dispatcher(S sender)
    {
        _sender = sender;
    }

    virtual public void Dispatch(T value)
    {
        if (observers != null)
            observers(_sender, value);
    }

    S  _sender;
}

