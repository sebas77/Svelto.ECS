namespace Svelto.ECS
{
    public abstract class GenericEntityDescriptor<T>:IEntityDescriptor where T : IEntityStruct, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] { new EntityViewBuilder<T>() };
        }
        
        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }

        static readonly IEntityViewBuilder[] entityViewBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U> : IEntityDescriptor     where T : IEntityStruct, new() 
                                                                       where U : IEntityStruct, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] {new EntityViewBuilder<T>(), new EntityViewBuilder<U>()};
        }

        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }

        static readonly IEntityViewBuilder[] entityViewBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V> : IEntityDescriptor     where T : IEntityStruct, new()
                                                                          where U : IEntityStruct, new()
                                                                          where V : IEntityStruct, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] {new EntityViewBuilder<T>(), new EntityViewBuilder<U>(), new EntityViewBuilder<V>()};
        }

        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }

        static readonly IEntityViewBuilder[] entityViewBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W> : IEntityDescriptor     where T : IEntityStruct, new()
                                                                             where U : IEntityStruct, new()
                                                                             where V : IEntityStruct, new()
                                                                             where W : IEntityStruct, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] {new EntityViewBuilder<T>(), new EntityViewBuilder<U>(), new EntityViewBuilder<V>(), new EntityViewBuilder<W>()};
        }

        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }

        static readonly IEntityViewBuilder[] entityViewBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W, X> : IEntityDescriptor     where T : IEntityStruct, new()
                                                                                where U : IEntityStruct, new()
                                                                                where V : IEntityStruct, new()
                                                                                where W : IEntityStruct, new()
                                                                                where X : IEntityStruct, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] {new EntityViewBuilder<T>(), new EntityViewBuilder<U>(), new EntityViewBuilder<V>(), new EntityViewBuilder<W>(), new EntityViewBuilder<X>()};
        }

        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }

        static readonly IEntityViewBuilder[] entityViewBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W, X, Y> : IEntityDescriptor     where T : IEntityStruct, new()
                                                                                   where U : IEntityStruct, new()
                                                                                   where V : IEntityStruct, new()
                                                                                   where W : IEntityStruct, new()
                                                                                   where X : IEntityStruct, new()
                                                                                   where Y : IEntityStruct, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] {new EntityViewBuilder<T>(), new EntityViewBuilder<U>(), new EntityViewBuilder<V>(), new EntityViewBuilder<W>(), new EntityViewBuilder<X>(), new EntityViewBuilder<Y>()};
        }

        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }

        static readonly IEntityViewBuilder[] entityViewBuilders;
    }
}
