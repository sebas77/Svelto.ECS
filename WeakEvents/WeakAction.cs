using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Svelto.WeakEvents
{
    public class WeakAction<T1, T2> : WeakAction
    {
        public WeakAction(Action<T1, T2> listener)
            : base(listener.Target, listener.GetMethodInfoEx())
        {}

        public void Invoke(T1 data1, T2 data2)
        {
            _data[0] = data1;
            _data[1] = data2;

            Invoke_Internal(_data);
        }

        readonly object[] _data = new object[2];
    }

    public class WeakAction<T> : WeakActionBase
    {
        public WeakAction(Action<T> listener)
            : base(listener.Target, listener.GetMethodInfoEx())
        {}

        public void Invoke(T data)
        {
            _data[0] = data;

            Invoke_Internal(_data);
        }

        readonly object[] _data = new object[1];
    }

    public class WeakAction : WeakActionBase
    {
        public WeakAction(Action listener) : base(listener)
        {}

        public WeakAction(object listener, MethodInfo method) : base(listener, method)
        {}

        public void Invoke()
        {
            Invoke_Internal(null);
        }
    }

    public abstract class WeakActionBase
    {
        protected readonly DataStructures.WeakReference<object> ObjectRef;
        protected readonly MethodInfo Method;

        public bool IsValid
        {
            get { return ObjectRef.IsValid; }
        }

        protected WeakActionBase(Action listener)
            : this(listener.Target, listener.GetMethodInfoEx())
        {}

        protected WeakActionBase(object listener, MethodInfo method)
        {
            ObjectRef = new DataStructures.WeakReference<object>(listener);

            Method = method;

#if NETFX_CORE
        var attributes = (CompilerGeneratedAttribute[])method.GetType().GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
        if (attributes.Length != 0)
            throw new ArgumentException("Cannot create weak event to anonymous method with closure.");
#else
            if (method.DeclaringType.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length != 0)
                throw new ArgumentException("Cannot create weak event to anonymous method with closure.");
#endif
        }

        protected void Invoke_Internal(object[] data)
        {
            if (ObjectRef.IsValid)
                Method.Invoke(ObjectRef.Target, data);
            else
                Utility.Console.LogWarning("Target of weak action has been garbage collected");
        }
    }
}