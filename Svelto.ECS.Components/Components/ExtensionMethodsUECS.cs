#if UNITY_MATHEMATICS
using System.Runtime.CompilerServices;
using UnityEngine;
using Svelto.ECS.Components;
using Unity.Mathematics;

public static partial class ExtensionMethods
{
    public static float3 ToFloat3(in this ECSVector3 vector)
    {
        return new float3(vector.x, vector.y, vector.z);
    }
        
    public static quaternion ToQuaternion(in this ECSVector4 vector)
    {
        return new quaternion(vector.x, vector.y, vector.z, vector.w);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Mul(ref this float3 vector1, float value)
    {
        vector1.x *= value;
        vector1.y *= value;
        vector1.z *= value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Div(ref this float3 vector1, float value)
    {
        vector1.x /= value;
        vector1.y /= value;
        vector1.z /= value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ProjectOnPlane(ref this float3 vector, in float3 planeNormal)
    {
        var num1 = math.dot(planeNormal,planeNormal);
        if ((double) num1 < (double) Mathf.Epsilon)
            return;
        var num2 = math.dot(vector,planeNormal) / num1;
        
        vector.x -= planeNormal.x * num2;
        vector.y -= planeNormal.y * num2;
        vector.z -= planeNormal.z * num2;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Normalize(ref this float3 x)
    {
        x.Mul(math.rsqrt(math.dot(x,x)));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NormalizeSafe(ref this float3 x)
    {
        var len = math.dot(x,x);
        x = len > math.FLT_MIN_NORMAL ? x * math.rsqrt(len) : float3.zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(ref this float3 a, in float3 b)
    {
        a.x += b.x;
        a.y += b.y;
        a.z += b.z;
    }

}

#endif