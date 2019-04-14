using System;

namespace Svelto.ECS
{
    public struct EntityStructInfoView: IEntityStruct, INeedEGID
    {
        public EGID ID   { get; set; }
        public Type type { get; set; }

        public IEntityBuilder[] entitiesToBuild;
    }
}