namespace Svelto.ECS
{
    public abstract class GenericEntityDescriptor<T>:IEntityDescriptor where T : struct, IEntityData
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

    public abstract class GenericEntityDescriptor<T, U> : IEntityDescriptor     where T : struct, IEntityData 
                                                                       where U : struct, IEntityData
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

    public abstract class GenericEntityDescriptor<T, U, V> : IEntityDescriptor     where T : struct, IEntityData
                                                                          where U : struct, IEntityData
                                                                          where V : struct, IEntityData
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

    public abstract class GenericEntityDescriptor<T, U, V, W> : IEntityDescriptor     where T : struct, IEntityData
                                                                             where U : struct, IEntityData
                                                                             where V : struct, IEntityData
                                                                             where W : struct, IEntityData
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

    public abstract class GenericEntityDescriptor<T, U, V, W, X> : IEntityDescriptor     where T : struct, IEntityData
                                                                                where U : struct, IEntityData
                                                                                where V : struct, IEntityData
                                                                                where W : struct, IEntityData
                                                                                where X : struct, IEntityData
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

    public abstract class GenericEntityDescriptor<T, U, V, W, X, Y> : IEntityDescriptor     where T : struct, IEntityData
                                                                                   where U : struct, IEntityData
                                                                                   where V : struct, IEntityData
                                                                                   where W : struct, IEntityData
                                                                                   where X : struct, IEntityData
                                                                                   where Y : struct, IEntityData
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
