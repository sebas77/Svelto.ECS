#if UNITY_5 || UNITY_5_3_OR_NEWER
using UnityEngine;

namespace Svelto.ECS.Components.Unity
{
    public struct ECSRect
    {
        public float x, y, width, height;

        public ECSRect(Rect imageUvRect)
        {
            x      = imageUvRect.x;
            y      = imageUvRect.y;
            width  = imageUvRect.width;
            height = imageUvRect.height;
        }
    }
}
#endif