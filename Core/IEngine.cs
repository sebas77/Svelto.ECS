using System;
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

    [Obsolete]
    public interface IReactOnAddAndRemove<T> : IReactOnAdd<T>, IReactOnRemove<T> where T : _IInternalEntityComponent
    {
    }

    /// <summary>
    /// Interface to mark an Engine as reacting on engines root disposed.
    /// It can work together with IReactOnRemove which normally is not called on enginesroot disposed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReactOnDispose<T> : IReactOnDispose where T : _IInternalEntityComponent
    {
        void Remove(ref T entityComponent, EGID egid);
    }

    /// <summary>
    /// Interface to mark an Engine as reacting to entities swapping group
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Obsolete]
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