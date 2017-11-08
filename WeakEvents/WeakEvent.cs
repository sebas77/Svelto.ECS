using Svelto.DataStructures;
using System;
using System.Reflection;

namespace Svelto.WeakEvents
{
    public class WeakEvent
    {
        public static WeakEvent operator+(WeakEvent c1, Action x)
        {
            if (c1 == null) c1 = new WeakEvent();
            c1._subscribers.Add(new WeakActionStruct(x));

            return c1;
        }

        public static WeakEvent operator-(WeakEvent c1, Action x)
        {
            DesignByContract.Check.Require(x != null);
            c1.Remove(x.Target, x.GetMethodInfoEx());

            return c1;
        }

        public void Invoke()
        {
            for (int i = 0; i < _subscribers.Count; i++)
                if (_subscribers[i].Invoke() == false)
                    _subscribers.UnorderedRemoveAt(i--);
        }

        void Remove(object thisObject, MethodInfo thisMethod)
        {
            for (int i = 0; i < _subscribers.Count; ++i)
            {
                var otherObject = _subscribers[i];

                if (otherObject.IsMatch(thisObject, thisMethod))
                {
                    _subscribers.UnorderedRemoveAt(i);
                    break;
                }
            }
        }

        ~WeakEvent()
        {
            for (int i = 0; i < _subscribers.Count; i++)
                _subscribers[i].Dispose();
        }

        readonly FasterList<WeakActionStruct> 
            _subscribers = new FasterList<WeakActionStruct>();
    }

    public class WeakEvent<T1>
    {
        public static WeakEvent<T1> operator+(WeakEvent<T1> c1, Action<T1> x)
        {
            if (c1 == null) c1 = new WeakEvent<T1>();
            c1._subscribers.Add(new WeakActionStruct<T1>(x));

            return c1;
        }

        public static WeakEvent<T1> operator-(WeakEvent<T1> c1, Action<T1> x)
        {
            DesignByContract.Check.Require(x != null);
            c1.Remove(x.Target, x.GetMethodInfoEx());

            return c1;
        }

        public void Invoke(T1 arg1)
        {
            args[0] = arg1;

            for (int i = 0; i < _subscribers.Count; i++)
                if (_subscribers[i].Invoke(args) == false)
                    _subscribers.UnorderedRemoveAt(i--);
        }

        void Remove(object thisObject, MethodInfo thisMethod)
        {
            for (int i = 0; i < _subscribers.Count; ++i)
            {
                var otherObject = _subscribers[i];
                
                if (otherObject.IsMatch(thisObject, thisMethod))
                {
                    _subscribers.UnorderedRemoveAt(i);
                    break;
                }
            }
        }

        ~WeakEvent()
        {
            for (int i = 0; i < _subscribers.Count; i++)
                _subscribers[i].Dispose();
        }

        readonly object[] args = new object[1];

        readonly FasterList<WeakActionStruct<T1>> 
            _subscribers = new FasterList<WeakActionStruct<T1>>();
    }

    public class WeakEvent<T1, T2>
    {
        public static WeakEvent<T1, T2> operator+(WeakEvent<T1, T2> c1, Action<T1, T2> x)
        {
            if (c1 == null) c1 = new WeakEvent<T1, T2>();
            c1._subscribers.Add(new WeakActionStruct<T1, T2>(x));

            return c1;
        }

        public static WeakEvent<T1, T2> operator-(WeakEvent<T1, T2> c1, Action<T1, T2> x)
        {
            DesignByContract.Check.Require(x != null);
            c1.Remove(x.Target, x.GetMethodInfoEx());

            return c1;
        }

        public void Invoke(T1 arg1, T2 arg2)
        {
            args[0] = arg1;
            args[1] = arg2;

            for (int i = 0; i < _subscribers.Count; i++)
                if (_subscribers[i].Invoke(args) == false)
                    _subscribers.UnorderedRemoveAt(i--);
        }

        void Remove(object thisObject, MethodInfo thisMethod)
        {
            for (int i = 0; i < _subscribers.Count; ++i)
            {
                var otherObject = _subscribers[i];

                if (otherObject.IsMatch(thisObject, thisMethod))
                {
                    _subscribers.UnorderedRemoveAt(i);

                    break;
                }
            }
        }

        ~WeakEvent()
        {
            for (int i = 0; i < _subscribers.Count; i++)
                _subscribers[i].Dispose();
        }

        readonly object[] args = new object[2];

        readonly FasterList<WeakActionStruct<T1, T2>> 
            _subscribers = new FasterList<WeakActionStruct<T1, T2>>();
    }
}
