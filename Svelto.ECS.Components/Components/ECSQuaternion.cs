namespace Svelto.ECS.Components
{
    public struct ECSQuaternion
    {
        public static readonly ECSQuaternion identity = new ECSQuaternion(0f, 0f, 0f, 1f);
        public float x, y, z, w;

        public ECSQuaternion(float X, float Y, float Z, float W)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }
    }
}
