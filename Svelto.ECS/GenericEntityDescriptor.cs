namespace Svelto.ECS
{
    public abstract class GenericEntityDescriptor<T>:IEntityDescriptor where T : IEntityData, new()
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

    public abstract class GenericEntityDescriptor<T, U> : IEntityDescriptor     where T : IEntityData, new() 
                                                                       where U : IEntityData, new()
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

    public abstract class GenericEntityDescriptor<T, U, V> : IEntityDescriptor     where T : IEntityData, new()
                                                                          where U : IEntityData, new()
                                                                          where V : IEntityData, new()
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

    public abstract class GenericEntityDescriptor<T, U, V, W> : IEntityDescriptor     where T : IEntityData, new()
                                                                             where U : IEntityData, new()
                                                                             where V : IEntityData, new()
                                                                             where W : IEntityData, new()
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

    public abstract class GenericEntityDescriptor<T, U, V, W, X> : IEntityDescriptor     where T : IEntityData, new()
                                                                                where U : IEntityData, new()
                                                                                where V : IEntityData, new()
                                                                                where W : IEntityData, new()
                                                                                where X : IEntityData, new()
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

    public abstract class GenericEntityDescriptor<T, U, V, W, X, Y> : IEntityDescriptor     where T : IEntityData, new()
                                                                                   where U : IEntityData, new()
                                                                                   where V : IEntityData, new()
                                                                                   where W : IEntityData, new()
                                                                                   where X : IEntityData, new()
                                                                                   where Y : IEntityData, new()
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
