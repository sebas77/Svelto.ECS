public class Dispatcher<S, T>
{
    public event System.Action<S, T> subscribers;

    private Dispatcher() { }

    public Dispatcher(S sender)
    {
        _sender = sender;
    }

    public void Dispatch(ref T value)
    {
        if (subscribers != null)
            subscribers(_sender, value);
    }

    readonly S  _sender;
}

public class Dispatcher<S>
{
    public event System.Action<S> subscribers;

    private Dispatcher() { }

    public Dispatcher(S sender)
    {
        _sender = sender;
    }

    public void Dispatch()
    {
        if (subscribers != null)
            subscribers(_sender);
    }

    readonly S  _sender;
}
