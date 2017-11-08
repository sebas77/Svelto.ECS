using System;
using System.Reflection;

public static class NetFXCoreWrappers
{
    public static MethodInfo GetMethodInfoEx(this Delegate delegateEx)
    {
#if NETFX_CORE
        var method = delegateEx.GetMethodInfo();
#else
        var method = delegateEx.Method;
#endif
        return method;
    }

    public static Type GetDeclaringType(this MethodInfo methodInfo)
    {
#if NETFX_CORE
        return methodInfo.DeclaringType;
#else
        return methodInfo.ReflectedType;
#endif
    }
}
