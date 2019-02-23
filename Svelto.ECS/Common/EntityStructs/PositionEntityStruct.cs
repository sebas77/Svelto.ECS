using Svelto.ECS.Components;

namespace Svelto.ECS.EntityStructs
{
    public struct PositionEntityStruct : IEntityStruct
    {
        public ECSVector3 position;

        public EGID ID { get; set; }
    }
}