using System;
using System.Runtime.CompilerServices;
using Svelto.ECS.Components;

public static partial class ExtensionMethods
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SqrMagnitude(in this ECSVector2 a) { return a.x * a.x + a.y * a.y; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SqrMagnitude(in this ECSVector3 a) { return a.x * a.x + a.y * a.y + a.z * a.z; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Magnitude(in this    ECSVector2 a) { return (float) Math.Sqrt(a.SqrMagnitude()); }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Magnitude(in this    ECSVector3 a) { return (float) Math.Sqrt(a.SqrMagnitude()); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(ref this ECSVector3 vector1, in ECSVector3 vector2)
    {
        vector1.x += vector2.x;
        vector1.y += vector2.y;
        vector1.z += vector2.z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(ref this ECSVector3 vector1, float x, float y, float z)
    {
        vector1.x += x;
        vector1.y += y;
        vector1.z += z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(ref this ECSVector3 vector1, float x, float y, float z)
    {
        vector1.x = x;
        vector1.y = y;
        vector1.z = z;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Zero(ref this ECSVector3 vector1)
    {
        vector1.x = 0;
        vector1.y = 0;
        vector1.z = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set(ref this ECSQuaternion quaternion, float x, float y, float z, float w)
    {
        quaternion.x = x;
        quaternion.y = y;
        quaternion.z = z;
        quaternion.w = w;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Interpolate(ref this ECSVector3 vector, in ECSVector3 vectorS, in ECSVector3 vectorE, float time)
    {
        vector.x = vectorS.x * (1 - time) + vectorE.x * time;
        vector.y = vectorS.y * (1 - time) + vectorE.y * time;
        vector.z = vectorS.z * (1 - time) + vectorE.z * time;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(ref this ECSVector3 vector1, in ECSVector3 vector2)
    {
        return vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ECSVector3 Cross(ref this ECSVector3 lhs, in ECSVector3 rhs)
    {
        return new ECSVector3(lhs.y * rhs.z - lhs.z * rhs.y, lhs.z * rhs.x - lhs.x * rhs.z,
                              lhs.x * rhs.y - lhs.y * rhs.x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ECSVector3 Mul(in this ECSQuaternion rotation, in ECSVector3 point)
    {
        float x  = rotation.x * 2F;
        float y  = rotation.y * 2F;
        float z  = rotation.z * 2F;
        float xx = rotation.x * x;
        float yy = rotation.y * y;
        float zz = rotation.z * z;
        float xy = rotation.x * y;
        float xz = rotation.x * z;
        float yz = rotation.y * z;
        float wx = rotation.w * x;
        float wy = rotation.w * y;
        float wz = rotation.w * z;

        return new ECSVector3((1F - (yy + zz)) * point.x + (xy - wz) * point.y + (xz + wy) * point.z,
            (xy + wz) * point.x + (1F - (xx + zz)) * point.y + (yz - wx) * point.z,
            (xz - wy) * point.x + (yz + wx) * point.y + (1F - (xx + yy)) * point.z);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap(ref this ECSVector3 vector, ref ECSVector3 vectorS)
    {
        float x = vector.x;
        float y = vector.y;
        float z = vector.z;

        vector.x = vectorS.x;
        vector.y = vectorS.y;
        vector.z = vectorS.z;

        vectorS.x = x;
        vectorS.y = y;
        vectorS.z = z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sub(ref this ECSVector3 vector1, in ECSVector3 vector2)
    {
        vector1.x -= vector2.x;
        vector1.y -= vector2.y;
        vector1.z -= vector2.z;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref ECSVector3 Mul(ref this ECSVector3 vector1, float value)
    {
        vector1.x *= value;
        vector1.y *= value;
        vector1.z *= value;

        return ref vector1;
    }
}
