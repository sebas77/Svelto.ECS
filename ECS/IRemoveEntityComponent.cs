using System;

namespace Svelto.ECS
{
    public interface IRemoveEntityComponent
    {
        Action removeEntity { get; set; }
    }

    public interface IDisableEntityComponent
    {
        Action disableEntity { get; set; }
    }

    public interface IEnableEntityComponent
    {
        Action enableEntity { get; set; }
    }
}
