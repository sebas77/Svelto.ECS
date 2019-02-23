#if UNITY_2018_3_OR_NEWER
using UnityEngine;

namespace Svelto.ECS.Components.Unity
{
    namespace Svelto.ECS.Components
    {
        public static class ExtensionMethods
        {
            public static Vector3 ToVector3(ref this ECSVector3 vector) { return new Vector3(vector.x, vector.y, vector.z); }

            public static void Add(ref this ECSVector3 vector1, ref ECSVector3 vector2)
            {
                vector1.x += vector2.x;
                vector1.y += vector2.y;
                vector1.z += vector2.z;
            }
            
            public static void Add(ref this ECSVector3 vector1, float x, float y, float z)
            {
                vector1.x += x;
                vector1.y += y;
                vector1.z += z;
            }
            
            public static void Interpolate(ref this ECSVector3 vector, ref ECSVector3 vectorS,
                ref ECSVector3 vectorE, float time)
            {
                vector.x = vectorS.x * (1 - time) + vectorE.x * (time);
                vector.y = vectorS.y * (1 - time) + vectorE.y * (time);
                vector.z = vectorS.z * (1 - time) + vectorE.z * (time);
            }
        }
    }
}
#endif