namespace Svelto.ECS
{
    public abstract class GenericEntityDescriptor<T> : IEntityDescriptor where T : struct,  IBaseEntityComponent
    {
        static readonly IComponentBuilder[] _componentBuilders;
        static GenericEntityDescriptor() { _componentBuilders = new IComponentBuilder[] {new ComponentBuilder<T>()}; }

        public IComponentBuilder[] componentsToBuild => _componentBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U> : IEntityDescriptor
        where T : struct,  IBaseEntityComponent where U : struct,  IBaseEntityComponent
    {
        static readonly IComponentBuilder[] _componentBuilders;

        static GenericEntityDescriptor()
        {
            _componentBuilders = new IComponentBuilder[] {new ComponentBuilder<T>(), new ComponentBuilder<U>()};
        }

        public IComponentBuilder[] componentsToBuild => _componentBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V> : IEntityDescriptor
        where T : struct,  IBaseEntityComponent where U : struct,  IBaseEntityComponent where V : struct,  IBaseEntityComponent
    {
        static readonly IComponentBuilder[] _componentBuilders;

        static GenericEntityDescriptor()
        {
            _componentBuilders = new IComponentBuilder[]
            {
                new ComponentBuilder<T>(),
                new ComponentBuilder<U>(),
                new ComponentBuilder<V>()
            };
        }

        public IComponentBuilder[] componentsToBuild => _componentBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W> : IEntityDescriptor
        where T : struct,  IBaseEntityComponent where U : struct,  IBaseEntityComponent where V : struct,  IBaseEntityComponent
        where W : struct,  IBaseEntityComponent
    {
        static readonly IComponentBuilder[] _componentBuilders;

        static GenericEntityDescriptor()
        {
            _componentBuilders = new IComponentBuilder[]
            {
                new ComponentBuilder<T>(),
                new ComponentBuilder<U>(),
                new ComponentBuilder<V>(),
                new ComponentBuilder<W>()
            };
        }

        public IComponentBuilder[] componentsToBuild => _componentBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W, X> : IEntityDescriptor
        where T : struct,  IBaseEntityComponent where U : struct,  IBaseEntityComponent where V : struct,  IBaseEntityComponent
        where W : struct,  IBaseEntityComponent where X : struct,  IBaseEntityComponent
    {
        static readonly IComponentBuilder[] _componentBuilders;

        static GenericEntityDescriptor()
        {
            _componentBuilders = new IComponentBuilder[]
            {
                new ComponentBuilder<T>(),
                new ComponentBuilder<U>(),
                new ComponentBuilder<V>(),
                new ComponentBuilder<W>(),
                new ComponentBuilder<X>()
            };
        }

        public IComponentBuilder[] componentsToBuild => _componentBuilders;
    }

    public abstract class GenericEntityDescriptor<T, U, V, W, X, Y> : IEntityDescriptor
        where T : struct,  IBaseEntityComponent where U : struct,  IBaseEntityComponent where V : struct,  IBaseEntityComponent
        where W : struct,  IBaseEntityComponent where X : struct,  IBaseEntityComponent where Y : struct,  IBaseEntityComponent
    {
        static readonly IComponentBuilder[] _componentBuilders;

        static GenericEntityDescriptor()
        {
            _componentBuilders = new IComponentBuilder[]
            {
                new ComponentBuilder<T>(),
                new ComponentBuilder<U>(),
                new ComponentBuilder<V>(),
                new ComponentBuilder<W>(),
                new ComponentBuilder<X>(),
                new ComponentBuilder<Y>()
            };
        }

        public IComponentBuilder[] componentsToBuild => _componentBuilders;
    }
}