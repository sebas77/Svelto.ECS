using System;

namespace Svelto.ECS
{
    public interface IEGIDMultiMapper
    {
        Type entityType { get; }

        uint GetIndex(EGID entity);
    }
}