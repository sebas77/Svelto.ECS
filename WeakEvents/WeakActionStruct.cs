using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

//careful, you must handle the destruction of the GCHandles!
namespace Svelto.WeakEvents
{
    public struct WeakActionStruct<T1, T2> : IEquatable<WeakActionStruct<T1, T2>>, IDisposable
    {
        public WeakActionStruct(Action<T1, T2> listener)
        {
            WeakActionStructUtility.Init(listener.Target, listener.GetMethodInfoEx(), out _objectRef, out _method);
        }

        public bool Invoke(object[] args)
        {
            return WeakActionStructUtility.Invoke(ref _objectRef, _method, args);
        }

        public bool Equals(WeakActionStruct<T1, T2> other)
        {
            return WeakActionStructUtility.IsMatch(_objectRef.Target, _method, 
                other._objectRef.Target, other._method);
        }

        public void Dispose()
        {
            _objectRef.Free();
        }

        public bool IsMatch(object otherObject, MethodInfo otherMethod)
        {
            return WeakActionStructUtility.IsMatch(_objectRef.Target, _method,
                otherObject, otherMethod);
        }

        GCHandle            _objectRef;
        readonly MethodInfo _method;
    }

    public struct WeakActionStruct<T> : IEquatable<WeakActionStruct<T>>, IDisposable
    {
        public WeakActionStruct(Action<T> listener)
        {
            WeakActionStructUtility.Init(listener.Target, listener.GetMethodInfoEx(), 
                out _objectRef, out _method);
        }

        public bool Invoke(object[] args)
        {
            return WeakActionStructUtility.Invoke(ref _objectRef, _method, args);
        }

        public bool Equals(WeakActionStruct<T> other)
        {
            return WeakActionStructUtility.IsMatch(_objectRef.Target, _method, 
                other._objectRef.Target, other._method);
        }

        public void Dispose()
        {
            _objectRef.Free();
        }

        public bool IsMatch(object otherObject, MethodInfo otherMethod)
        {
            return WeakActionStructUtility.IsMatch(_objectRef.Target, _method,
                otherObject, otherMethod);
        }

        GCHandle            _objectRef;
        readonly MethodInfo _method;
    }

    public struct WeakActionStruct : IEquatable<WeakActionStruct>, IDisposable
    {
        public WeakActionStruct(Action listener)
        {
            WeakActionStructUtility.Init(listener.Target, listener.GetMethodInfoEx(), 
                out _objectRef, out _method);
        }

        public bool Invoke()
        {
            return WeakActionStructUtility.Invoke(ref _objectRef, _method, null);
        }

        public bool Equals(WeakActionStruct other)
        {
            return WeakActionStructUtility.IsMatch(_objectRef.Target, _method, 
                other._objectRef.Target, other._method);
        }

        public void Dispose()
        {
            _objectRef.Free();
        }

        public bool IsMatch(object otherObject, MethodInfo otherMethod)
        {
            return WeakActionStructUtility.IsMatch(_objectRef.Target, _method,
                otherObject, otherMethod);
        }

        GCHandle            _objectRef;
        readonly MethodInfo _method;
    }

    static class WeakActionStructUtility
    {
        internal static void Init(object target, MethodInfo method, 
            out GCHandle objectRef, out MethodInfo methodOut)
        {
            objectRef = GCHandle.Alloc(target, GCHandleType.Weak);
            methodOut = method;

#if DEBUG && !PROFILER
#if NETFX_CORE
            Method = listener.GetMethodInfo();
            var attributes = (CompilerGeneratedAttribute[])Method.GetType().GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
            if(attributes.Length != 0)
                throw new ArgumentException("Cannot create weak event to anonymous method with closure.");
#else
            if (method.DeclaringType.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length != 0)
                throw new ArgumentException("Cannot create weak event to anonymous method with closure.");
#endif
#endif
        }

        public static bool Invoke(ref GCHandle objectRef, MethodInfo method, object[] args)
        {
            if (objectRef.IsAllocated && objectRef.Target != null)
            {
                method.Invoke(objectRef.Target, args);
                return true;
            }

            Dispose(ref objectRef);
            return false;
        }

        public static void Dispose(ref GCHandle objectRef)
        {
            objectRef.Free();
        }

        public static bool IsMatch(object objectRef, MethodInfo method, 
            object _objectRef, MethodInfo _method)
        {
            return _method.Equals(method) && objectRef.Equals(_objectRef);
        }
    }
}