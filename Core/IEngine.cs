using Svelto.ECS.Internal;

namespace Svelto.ECS.Internal
{
    public interface IReactEngine : IEngine
    {
    }
#region legacy interfaces
    /// <summary>
    /// This is now considered legacy and it will be deprecated in future
    /// </summary>
    public interface IReactOnAdd : IReactEngine
    {
    }
    
    /// <summary>
    /// This is now considered legacy and it will be deprecated in future
    /// </summary>
    public interface IReactOnRemove : IReactEngine
    {
    }
    
    /// <summary>
    /// This is now considered legacy and it will be deprecated in future
    /// </summary>
    public interface IReactOnSwap : IReactEngine
    {
    }
#endregion    

    public interface IReactOnAddEx : IReactEngine
    {
    }

    public interface IReactOnRemoveEx : IReactEngine
    {
    }

    public interface IReactOnSwapEx : IReactEngine
    {
    }

    public interface IReactOnDispose : IReactEngine
    {
    }
}

namespace Svelto.ECS
{
    public interface IEngine
    {
    }

    public interface IGetReadyEngine : IEngine
    {
        void Ready();
    }

    public interface IQueryingEntitiesEngine : IGetReadyEngine
    {
        EntitiesDB entitiesDB { set; }
    }

    /// <summary>
    /// Interface to mark an Engine as reacting on entities added
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactOnAdd<T> : IReactOnAdd where T : IEntityComponent
    {
        void Add(ref T entityComponent, EGID egid);
    }

    public interface IReactOnAddEx<T> : IReactOnAddEx where T : struct, IEntityComponent
    {
        void Add((uint start, uint end) rangeOfEntities, in EntityCollection<T> collection,
            ExclusiveGroupStruct groupID);
    }

    /// <summary>
    /// Interface to mark an Engine as reacting on entities removed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactOnRemove<T> : IReactOnRemove where T : IEntityComponent
    {
        void Remove(ref T entityComponent, EGID egid);
    }

    public interface IReactOnRemoveEx<T> : IReactOnRemoveEx where T : struct, IEntityComponent
    {
        void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<T> collection,
            ExclusiveGroupStruct groupID);
    }

    public interface IReactOnAddAndRemove<T> : IReactOnAdd<T>, IReactOnRemove<T> where T : IEntityComponent
    {
    }

    /// <summary>
    /// Interface to mark an Engine as reacting on engines root disposed.
    /// It can work together with IReactOnRemove which normally is not called on enginesroot disposed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactOnDispose<T> : IReactOnDispose where T : IEntityComponent
    {
        void Remove(ref T entityComponent, EGID egid);
    }

    /// <summary>
    /// Interface to mark an Engine as reacting to entities swapping group
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactOnSwap<T> : IReactOnSwap where T : IEntityComponent
    {
        void MovedTo(ref T entityComponent, ExclusiveGroupStruct previousGroup, EGID egid);
    }

    public interface IReactOnSwapEx<T> : IReactOnSwapEx where T : struct, IEntityComponent
    {
        void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<T> collection,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup);
    }

    /// <summary>
    /// Interface to mark an Engine as reacting after each entities submission phase
    /// </summary>
    public interface IReactOnSubmission : IReactEngine
    {
        void EntitiesSubmitted();
    }
}