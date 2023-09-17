using System;

namespace Svelto.ECS
{
    public interface IEGIDMultiMapper
    {
        uint GetIndex(EGID entity);
    }
}