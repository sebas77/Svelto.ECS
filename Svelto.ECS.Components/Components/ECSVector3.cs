namespace Svelto.ECS.Components
{
    public struct ECSVector3
    {
        public float x, y, z;

        public static readonly ECSVector3 forward = new ECSVector3(0f, 0f, 1f);
        public static readonly ECSVector3 back = new ECSVector3(0f, 0f, -1f);
        public static readonly ECSVector3 right = new ECSVector3(1f, 0f, 0f);
        public static readonly ECSVector3 left = new ECSVector3(-1f, 0f, 0f);
        public static readonly ECSVector3 up = new ECSVector3(0f, 1f, 0f);
        public static readonly ECSVector3 down = new ECSVector3(0f, -1f, 0f);

        public static readonly ECSVector3 zero = new ECSVector3(0f, 0f, 0f);
        public static readonly ECSVector3 one = new ECSVector3(1f, 1f, 1f);

        public ECSVector3(float X, float Y, float Z)
        {
            x = X;
            y = Y;
            z = Z;
        }
   }
}