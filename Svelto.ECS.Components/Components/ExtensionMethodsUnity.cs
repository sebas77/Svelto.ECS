#if UNITY_2018_3_OR_NEWER
using Svelto.ECS.Components;
using UnityEngine;

    public static partial class ExtensionMethods
    {
        public static Vector2    ToVector2(in this    ECSVector2 vector) { return new Vector2(vector.x, vector.y); }
        public static ECSVector2 ToECSVector2(in this Vector2    vector) { return new ECSVector2(vector.x, vector.y); }

        public static Vector3 ToVector3(in this ECSVector3 vector)
        {
            return new Vector3(vector.x, vector.y, vector.z);
        }
        public static ECSVector3 ToECSVector3(in this Vector3 vector)
        {
            return new ECSVector3(vector.x, vector.y, vector.z);
        }

        public static Quaternion ToQuaternion(in this ECSQuaternion quaternion)
        {
            return new Quaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }
        public static ECSQuaternion ToECSQuaternion(in this Quaternion quaternion)
        {
            return new ECSQuaternion(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        }

        public static void Set(ref this ECSQuaternion ecsQuaternion, in Quaternion quaternion)
        {
            ecsQuaternion.x = quaternion.x;
            ecsQuaternion.y = quaternion.y;
            ecsQuaternion.z = quaternion.z;
            ecsQuaternion.w = quaternion.w;
        }

        public static void Set(ref this ECSVector3 ecsVector3, in Vector3 vector3)
        {
            ecsVector3.x = vector3.x;
            ecsVector3.y = vector3.y;
            ecsVector3.z = vector3.z;
        }
    }

#endif