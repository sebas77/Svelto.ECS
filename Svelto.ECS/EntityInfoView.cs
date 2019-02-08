using System;

namespace Svelto.ECS
{
    public struct EntityInfoView : IEntityStruct
    {
        public EGID ID   { get; set; }
        public Type type { get; set; }

        public IEntityBuilder[] entitiesToBuild;
    }
}