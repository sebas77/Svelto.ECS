using Svelto.ECS.Components;

namespace Svelto.ECS.EntityStructs
{
    public struct RotationEntityStruct : IEntityStruct
    {
        public EcsVector4 rotation;
        
        public EGID ID { get; set; }
    }
}