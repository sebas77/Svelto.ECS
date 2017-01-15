using System;

namespace Svelto.ECS
{
    public interface IRemoveEntityComponent
    {
        Action removeEntity { get; set; }
    }
}
