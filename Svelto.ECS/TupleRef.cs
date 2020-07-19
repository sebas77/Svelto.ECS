namespace Svelto.ECS
{
    public readonly ref struct TupleRef<T1> where T1 : struct, IEntityComponent
    {
        public readonly EntityCollections<T1> entities;
        public readonly GroupsEnumerable<T1>  groups;

        public TupleRef(in EntityCollections<T1> entityCollections, in GroupsEnumerable<T1> groupsEnumerable)
        {
            this.entities = entityCollections;
            groups        = groupsEnumerable;
        }
    }

    public readonly ref struct TupleRef<T1, T2> where T1 : struct, IEntityComponent where T2 : struct, IEntityComponent
    {
        public readonly EntityCollections<T1, T2> entities;
        public readonly GroupsEnumerable<T1, T2>  groups;

        public TupleRef(in EntityCollections<T1, T2> entityCollections, in GroupsEnumerable<T1, T2> groupsEnumerable)
        {
            this.entities = entityCollections;
            groups        = groupsEnumerable;
        }
    }

    public readonly ref struct TupleRef<T1, T2, T3> where T1 : struct, IEntityComponent
                                                    where T2 : struct, IEntityComponent
                                                    where T3 : struct, IEntityComponent
    {
        public readonly EntityCollections<T1, T2, T3> entities;
        public readonly GroupsEnumerable<T1, T2, T3>  groups;

        public TupleRef
            (in EntityCollections<T1, T2, T3> entityCollections, in GroupsEnumerable<T1, T2, T3> groupsEnumerable)
        {
            this.entities = entityCollections;
            groups        = groupsEnumerable;
        }
    }
    
    public readonly ref struct TupleRef<T1, T2, T3, T4> where T1 : struct, IEntityComponent
                                                        where T2 : struct, IEntityComponent
                                                        where T3 : struct, IEntityComponent
                                                        where T4 : struct, IEntityComponent
    {
        public readonly EntityCollections<T1, T2, T3, T4> entities;
        public readonly GroupsEnumerable<T1, T2, T3, T4>  groups;

        public TupleRef
        (in EntityCollections<T1, T2, T3, T4> entityCollections
       , in GroupsEnumerable<T1, T2, T3, T4> groupsEnumerable)
        {
            this.entities = entityCollections;
            groups        = groupsEnumerable;
        }
    }
}