namespace Svelto.ECS
{
    public class DispatcherOnSet<T>
    {
        public DispatcherOnSet(int senderID)
        {
            _senderID = senderID;
            _subscribers = new System.Collections.Generic.HashSet<WeakAction<int, T>>();
        }

        public T value
        {
            set
            {
                _value = value;

                if (_subscribers != null)
                {
                    using (var enumerator = _subscribers.GetEnumerator())
                    {
                        while (enumerator.MoveNext() == true)
                            enumerator.Current.Invoke(_senderID, _value);
                    }
                }
            }

            get 
            {
                return _value;
            }
        }

        public void NotifyOnDataChange(System.Action<int, T> action)
        {
            _subscribers.Add(new WeakAction<int, T>(action));
        }

        public void StopNotifyOnDataChange(System.Action<int, T> action)
        {
            _subscribers.Remove(new WeakAction<int, T>(action));
        }

        protected T      _value;
        protected int    _senderID;

        protected System.Collections.Generic.HashSet<WeakAction<int, T>> _subscribers;
    }
}
