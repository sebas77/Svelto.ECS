using System;

namespace Svelto.ECS.NodeSchedulers
{
    public abstract class NodeSubmissionScheduler
    {
        abstract public void Schedule(Action submitNodes);
    }
}