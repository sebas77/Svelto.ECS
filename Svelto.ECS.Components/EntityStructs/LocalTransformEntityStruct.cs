using Svelto.ECS.Components;

namespace Svelto.ECS.EntityStructs
{
    public struct LocalTransformEntityStruct : IEntityStruct
    {
        public ECSVector3 position;
        public ECSQuaternion rotation;
    }
}
