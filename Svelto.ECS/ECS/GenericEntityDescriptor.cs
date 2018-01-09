using System.Runtime.InteropServices;

namespace Svelto.ECS
{
    public class GenericEntityDescriptor<T>:IEntityDescriptor where T : EntityView, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] { new EntityViewBuilder<T>() };
        }
        
        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }

        public static readonly IEntityViewBuilder[] entityViewBuilders;
        
    }

    public class GenericEntityDescriptor<T, U> : IEntityDescriptor     where T : EntityView, new() 
                                                                       where U : EntityView, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] {new EntityViewBuilder<T>(), new EntityViewBuilder<U>()};
        }

        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }
        
        public static readonly IEntityViewBuilder[] entityViewBuilders;
    }

    public class GenericEntityDescriptor<T, U, V> : IEntityDescriptor     where T : EntityView, new()
                                                                          where U : EntityView, new()
                                                                          where V : EntityView, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] {new EntityViewBuilder<T>(), new EntityViewBuilder<U>(), new EntityViewBuilder<V>()};
        }

        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }
        
        public static readonly IEntityViewBuilder[] entityViewBuilders;
    }

    public class GenericEntityDescriptor<T, U, V, W> : IEntityDescriptor     where T : EntityView, new()
                                                                             where U : EntityView, new()
                                                                             where V : EntityView, new()
                                                                             where W : EntityView, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] {new EntityViewBuilder<T>(), new EntityViewBuilder<U>(), new EntityViewBuilder<V>(), new EntityViewBuilder<W>()};
        }

        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }
        
        public static readonly IEntityViewBuilder[] entityViewBuilders;
    }

    public class GenericEntityDescriptor<T, U, V, W, X> : IEntityDescriptor     where T : EntityView, new()
                                                                                where U : EntityView, new()
                                                                                where V : EntityView, new()
                                                                                where W : EntityView, new()
                                                                                where X : EntityView, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] {new EntityViewBuilder<T>(), new EntityViewBuilder<U>(), new EntityViewBuilder<V>(), new EntityViewBuilder<W>(), new EntityViewBuilder<X>()};
        }

        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }
        
        public static readonly IEntityViewBuilder[] entityViewBuilders;
    }

    public class GenericEntityDescriptor<T, U, V, W, X, Y> : IEntityDescriptor     where T : EntityView, new()
                                                                                   where U : EntityView, new()
                                                                                   where V : EntityView, new()
                                                                                   where W : EntityView, new()
                                                                                   where X : EntityView, new()
                                                                                   where Y : EntityView, new()
    {
        static GenericEntityDescriptor()
        {
            entityViewBuilders = new IEntityViewBuilder[] {new EntityViewBuilder<T>(), new EntityViewBuilder<U>(), new EntityViewBuilder<V>(), new EntityViewBuilder<W>(), new EntityViewBuilder<X>(), new EntityViewBuilder<Y>()};
        }

        public IEntityViewBuilder[] entityViewsToBuild
        {
            get { return entityViewBuilders; }
        }
        
        public static readonly IEntityViewBuilder[] entityViewBuilders;
    }
}
