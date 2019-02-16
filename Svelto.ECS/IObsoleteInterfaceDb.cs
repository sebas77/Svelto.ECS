using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface IObsoleteInterfaceDb
    {
        /// <summary>
        /// All the EntityView related methods are left for back compatibility, but
        /// shouldn't be used anymore. Always pick EntityViewStruct or EntityStruct
        /// over EntityView
        /// </summary>
        [Obsolete]
        ReadOnlyCollectionStruct<T> QueryEntityViews<T>(int group) where T : class, IEntityStruct;
        [Obsolete]
        ReadOnlyCollectionStruct<T> QueryEntityViews<T>(ExclusiveGroup.ExclusiveGroupStruct group) where T : class, IEntityStruct;
        /// <summary>
        /// All the EntityView related methods are left for back compatibility, but
        /// shouldn't be used anymore. Always pick EntityViewStruct or EntityStruct
        /// over EntityView
        /// </summary>
        [Obsolete]
        bool TryQueryEntityView<T>(EGID egid, out T entityView) where T : class, IEntityStruct;
        [Obsolete]
        bool TryQueryEntityView<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group, out T entityView) where T : class, IEntityStruct;
        /// <summary>
        /// All the EntityView related methods are left for back compatibility, but
        /// shouldn't be used anymore. Always pick EntityViewStruct or EntityStruct
        /// over EntityView
        /// </summary>
        [Obsolete]
        T QueryEntityView<T>(EGID egid) where T : class, IEntityStruct;
        [Obsolete]
        T QueryEntityView<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group) where T : class, IEntityStruct;
        /// <summary>
        /// ECS is meant to work on a set of Entities. Working on a single entity is sometime necessary, but using
        /// the following functions inside a loop would be a mistake as performance can be significantly impacted
        /// return the buffer and the index of the entity inside the buffer using the input EGID 
        /// </summary>
        /// <param name="entityGid"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete]
        T[] QueryEntitiesAndIndex<T>(EGID entityGid, out uint index) where T : IEntityStruct;
        [Obsolete]
        T[] QueryEntitiesAndIndex<T>(int id, ExclusiveGroup.ExclusiveGroupStruct group, out uint index) where T : IEntityStruct;
        [Obsolete]
        T[] QueryEntitiesAndIndex<T>(int id, int group, out uint index) where T : IEntityStruct;
    }
}