using Svelto.DataStructures;
using System;

namespace BetterWeakEvents
{
    public class WeakEvent<T1, T2>
    {
        public static WeakEvent<T1, T2> operator+(WeakEvent<T1, T2> c1, Action<T1, T2> x)
        {
            c1._subscribers.Add(new WeakAction<T1, T2>(x));
            return c1;
        }

        public static WeakEvent<T1, T2> operator-(WeakEvent<T1, T2> c1, Action<T1, T2> x)
        {
            c1._subscribers.UnorderredRemove(new WeakAction<T1, T2>(x));
            return c1;
        }

        public void Invoke(T1 arg1, T2 arg2)
        {
            for (int i = 0; i < _subscribers.Count; i++)
                if (_subscribers[i].Invoke(arg1, arg2) == false)
                    _subscribers.UnorderredRemoveAt(i--);
        }

        ~WeakEvent()
        {
            for (int i = 0; i < _subscribers.Count; i++)
                _subscribers[i].Release();
        }

        protected FasterList<WeakAction<T1, T2>> _subscribers = new FasterList<WeakAction<T1, T2>>();
    }
}