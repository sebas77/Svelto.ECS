using Svelto.ECS.Components;

namespace Svelto.ECS.EntityStructs
{
    public struct RotationEntityStruct : IEntityStruct
    {
        public ECSQuaternion rotation;
    }
}