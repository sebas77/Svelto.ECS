using UnityEngine;

namespace Svelto.ECS.Components
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