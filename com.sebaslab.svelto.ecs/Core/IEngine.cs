using System;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IReactEngine : IEngine
    {
    }

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
    
    /// <summary>
    /// This is now considered legacy and it will be deprecated in future
    /// </summary>
    public interface IReactOnDispose : IReactEngine
    {
    }

    public interface IReactOnAddEx : IReactEngine
    {
    }

    public interface IReactOnRemoveEx : IReactEngine
    {
    }

    public interface IReactOnSwapEx : IReactEngine
    {
    }
    
    public interface IReactOnDisposeEx : IReactEngine
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
        //Ready is a callback that can be used to signal that the engine is ready to be used because the entitiesDB is now available
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
    [Obsolete]
    public interface IReactOnAdd<T> : IReactOnAdd where T : _IInternalEntityComponent
    {
        void Add(ref T entityComponent, EGID egid);
    }

    public interface IReactOnAddEx<T> : IReactOnAddEx where T : struct, _IInternalEntityComponent
    {
        void Add((uint start, uint end) rangeOfEntities, in EntityCollection<T> entities,
            ExclusiveGroupStruct groupID);
    }

    /// <summary>
    /// Interface to mark an Engine as reacting on entities removed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete]
    public interface IReactOnRemove<T> : IReactOnRemove where T : _IInternalEntityComponent
    {
        void Remove(ref T entityComponent, EGID egid);
    }
    
    public interface IReactOnAddAndRemoveEx<T> : IReactOnAddEx<T>, IReactOnRemoveEx<T> where T : struct, _IInternalEntityComponent 
    {
    }

    public interface IReactOnRemoveEx<T> : IReactOnRemoveEx where T : struct, _IInternalEntityComponent
    {
        void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<T> entities,
            ExclusiveGroupStruct groupID);
    }

    [Obsolete("Use IReactOnAddEx<T> and IReactOnRemoveEx<T> or IReactOnAddAndRemoveEx<T> instead")]
    public interface IReactOnAddAndRemove<T> : IReactOnAdd<T>, IReactOnRemove<T> where T : _IInternalEntityComponent
    {
    }

    /// <summary>
    /// Interface to mark an Engine as reacting on engines root disposed.
    /// It can work together with IReactOnRemove which normally is not called on enginesroot disposed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("Use IReactOnDisposeEx<T> instead")]
    public interface IReactOnDispose<T> : IReactOnDispose where T : _IInternalEntityComponent
    {
        void Remove(ref T entityComponent, EGID egid);
    }

    /// <summary>
    /// Interface to mark an Engine as reacting to entities swapping group
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete("Use IReactOnSwapEx<T> instead")]
    public interface IReactOnSwap<T> : IReactOnSwap where T : _IInternalEntityComponent
    {
        void MovedTo(ref T entityComponent, ExclusiveGroupStruct previousGroup, EGID egid);
    }

    /// <summary>
    /// All the entities have been already submitted in the database (swapped) when this callback is triggered
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactOnSwapEx<T> : IReactOnSwapEx where T : struct, _IInternalEntityComponent
    {
        void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<T> entities,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup);
    }
    
    public interface IReactOnDisposeEx<T> : IReactOnDisposeEx where T : struct, _IInternalEntityComponent
    {
        void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<T> entities,
            ExclusiveGroupStruct groupID);
    }

    /// <summary>
    /// Interface to mark an Engine as reacting after each entities submission phase
    /// </summary>
    public interface IReactOnSubmission : IReactEngine
    {
        void EntitiesSubmitted();
    }
    
    public interface IReactOnSubmissionStarted : IReactEngine
    {
        void EntitiesSubmissionStarting();
    }
}