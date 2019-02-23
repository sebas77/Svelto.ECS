using Svelto.ECS.Components;

namespace Svelto.ECS.EntityStructs
{
        public struct InterpolateVector3EntityStruct : IEntityStruct
    {
        public ECSVector3 starPos, endPos;
        public float time;

        public EGID ID { get; set; }
    }
}    