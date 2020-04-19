using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public static class EntityDBExtensions
    {
        public static NativeGroupsEnumerable<T1, T2> NativeGroupsIterator<T1, T2>(this EntitiesDB db,
                                                                                  ExclusiveGroupStruct[] groups)
            where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
        {
            return new NativeGroupsEnumerable<T1, T2>(db, groups);
        }

        public static NativeGroupsEnumerable<T1, T2, T3> NativeGroupsIterator
            <T1, T2, T3>(this EntitiesDB db, ExclusiveGroupStruct[] groups)
            where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
        {
            return new NativeGroupsEnumerable<T1, T2, T3>(db, groups);
        }
        
        public static NativeGroupsEnumerable<T1, T2, T3, T4> NativeGroupsIterator
            <T1, T2, T3, T4>(this EntitiesDB db, ExclusiveGroupStruct[] groups)
            where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent where T4 : unmanaged, IEntityComponent
        {
            return new NativeGroupsEnumerable<T1, T2, T3, T4>(db, groups);
        }
        
        public static NativeGroupsEnumerable<T1> NativeGroupsIterator<T1>(this EntitiesDB db, ExclusiveGroupStruct[] groups)
            where T1 : unmanaged, IEntityComponent
        {
            return new NativeGroupsEnumerable<T1>(db, groups);
        }

        public static NativeAllGroupsEnumerable<T1> NativeGroupsIterator<T1>(this EntitiesDB db)
            where T1 : unmanaged, IEntityComponent
        {
            return new NativeAllGroupsEnumerable<T1>(db);
        }
#if TO_BE_FINISHED       
        public static NativeAllGroupsEnumerable<T1, T2> NativeGroupsIterator<T1, T2>(this EntitiesDB db)
            where T1 : unmanaged, IEntityComponent where T2 : unmanaged, IEntityComponent
        {
            return new NativeAllGroupsEnumerable<T1, T2>(db);
        }
#endif
        public static NB<T> NativeEntitiesBuffer<T>(this EntitiesDB db, ExclusiveGroupStruct @group)
            where T : unmanaged, IEntityComponent
        {
            return db.QueryEntities<T>(group).ToNativeBuffer<T>();
        }
        
        public static BT<NB<T1>, NB<T2>> NativeEntitiesBuffer<T1, T2>(this EntitiesDB db, ExclusiveGroupStruct @group)
            where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
        {
            return db.QueryEntities<T1, T2>(group).ToNativeBuffers<T1, T2>();
        }
        
        public static BT<NB<T1>, NB<T2>, NB<T3>> NativeEntitiesBuffer<T1, T2, T3>(this EntitiesDB db, ExclusiveGroupStruct @group)
            where T1 : unmanaged, IEntityComponent
            where T2 : unmanaged, IEntityComponent
            where T3 : unmanaged, IEntityComponent
        {
            return db.QueryEntities<T1, T2, T3>(group).ToNativeBuffers<T1, T2, T3>();
        }
    }
}