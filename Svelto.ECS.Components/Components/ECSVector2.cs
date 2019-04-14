namespace Svelto.ECS.Components
{
    public struct ECSVector2
    {
        public float x, y;

        public static readonly ECSVector2 right = new ECSVector2(1f, 0f);
        public static readonly ECSVector2 left = new ECSVector2(-1f, 0f);
        public static readonly ECSVector2 down = new ECSVector2(0f, -1f);
        public static readonly ECSVector2 up = new ECSVector2(0f, 1f);
        public static readonly ECSVector2 one = new ECSVector2(1f, 1f);
        public static readonly ECSVector2 zero = new ECSVector2(0f, 0f);

        public ECSVector2(float X, float Y)
        {
            x = X;
            y = Y;
        }
    }
}