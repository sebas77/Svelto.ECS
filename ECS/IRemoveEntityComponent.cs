using System;

namespace Svelto.ES
{
    public interface IRemoveEntityComponent
    {
        Action removeEntity { get; set; }
    }
}
