namespace Svelto.ECS
{
    public class GenericEntityDescriptor<T> : EntityDescriptor
        where T : NodeWithID, new()
    {
        static GenericEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[] { new NodeBuilder<T>() };
        }
        public GenericEntityDescriptor(params object[] componentsImplementor) : base(_nodesToBuild, componentsImplementor)
        {}

        static readonly INodeBuilder[] _nodesToBuild;
    }

    public class GenericEntityDescriptor<T, U> : EntityDescriptor
        where T : NodeWithID, new()
        where U : NodeWithID, new()
    {
        static GenericEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[]
            {
                new NodeBuilder<T>(),
                new NodeBuilder<U>()
            };
        }

        public GenericEntityDescriptor(params object[] componentsImplementor) : base(_nodesToBuild, componentsImplementor)
        {}

        static readonly INodeBuilder[] _nodesToBuild;
    }

    public class GenericEntityDescriptor<T, U, V> : EntityDescriptor
        where T : NodeWithID, new()
        where U : NodeWithID, new()
        where V : NodeWithID, new()
    {
        static GenericEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[]
            {
                new NodeBuilder<T>(),
                new NodeBuilder<U>(),
                new NodeBuilder<V>()
            };
        }
        public GenericEntityDescriptor(params object[] componentsImplementor) : base(_nodesToBuild, componentsImplementor)
        {}

        static readonly INodeBuilder[] _nodesToBuild;
    }

    public class GenericEntityDescriptor<T, U, V, W> : EntityDescriptor
        where T : NodeWithID, new()
        where U : NodeWithID, new()
        where V : NodeWithID, new()
        where W : NodeWithID, new()
    {
        static GenericEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[]
            {
                new NodeBuilder<T>(),
                new NodeBuilder<U>(),
                new NodeBuilder<V>(),
                new NodeBuilder<W>()
            };
        }
        public GenericEntityDescriptor(params object[] componentsImplementor) : base(_nodesToBuild, componentsImplementor)
        {}

        static readonly INodeBuilder[] _nodesToBuild;
    }

    public class GenericEntityDescriptor<T, U, V, W, X> : EntityDescriptor
        where T : NodeWithID, new()
        where U : NodeWithID, new()
        where V : NodeWithID, new()
        where W : NodeWithID, new()
        where X : NodeWithID, new()
    {
        static GenericEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[]
            {
                new NodeBuilder<T>(),
                new NodeBuilder<U>(),
                new NodeBuilder<V>(),
                new NodeBuilder<W>(),
                new NodeBuilder<X>()
            };
        }
        public GenericEntityDescriptor(params object[] componentsImplementor) : base(_nodesToBuild, componentsImplementor)
        {}

        static readonly INodeBuilder[] _nodesToBuild;
    }

    public class GenericEntityDescriptor<T, U, V, W, X, Y> : EntityDescriptor
        where T : NodeWithID, new()
        where U : NodeWithID, new()
        where V : NodeWithID, new()
        where W : NodeWithID, new()
        where X : NodeWithID, new()
        where Y : NodeWithID, new()
    {
        static GenericEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[]
            {
                new NodeBuilder<T>(),
                new NodeBuilder<U>(),
                new NodeBuilder<V>(),
                new NodeBuilder<W>(),
                new NodeBuilder<X>(),
                new NodeBuilder<Y>()
            };
        }
        public GenericEntityDescriptor(params object[] componentsImplementor) : base(_nodesToBuild, componentsImplementor)
        {}

        static readonly INodeBuilder[] _nodesToBuild;
    }

    public class GenericMixedEntityDescriptor<T> : EntityDescriptor
        where T : INodeBuilder, new()
    {
        static GenericMixedEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[]
            {
                new T(),
            };
        }
        public GenericMixedEntityDescriptor(params object[] componentsImplementor) :
            base(_nodesToBuild, componentsImplementor)
        { }

        static readonly INodeBuilder[] _nodesToBuild;
    }

    public class GenericMixedEntityDescriptor<T, U, V> : EntityDescriptor
        where T : INodeBuilder, new()
        where U : INodeBuilder, new()
        where V : INodeBuilder, new()
    {
        static GenericMixedEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[]
            {
                new T(),
                new U(),
                new V()
            };
        }
        public GenericMixedEntityDescriptor(params object[] componentsImplementor) :
            base(_nodesToBuild, componentsImplementor)
        { }

        static readonly INodeBuilder[] _nodesToBuild;
    }

    public class GenericMixedEntityDescriptor<T, U, V, W> : EntityDescriptor
        where T : INodeBuilder, new()
        where U : INodeBuilder, new()
        where V : INodeBuilder, new()
        where W : INodeBuilder, new()
    {
        static GenericMixedEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[]
            {
                new T(),
                new U(),
                new V(),
                new W()
            };
        }
        public GenericMixedEntityDescriptor(params object[] componentsImplementor) :
            base(_nodesToBuild, componentsImplementor)
        { }

        static readonly INodeBuilder[] _nodesToBuild;
    }

    public class GenericMixedEntityDescriptor<T, U, V, W, X> : EntityDescriptor
        where T : INodeBuilder, new()
        where U : INodeBuilder, new()
        where V : INodeBuilder, new()
        where W : INodeBuilder, new()
        where X : INodeBuilder, new()
    {
        static GenericMixedEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[]
            {
                new T(),
                new U(),
                new V(),
                new W(),
                new X()
            };
        }
        public GenericMixedEntityDescriptor(params object[] componentsImplementor) :
            base(_nodesToBuild, componentsImplementor)
        { }

        static readonly INodeBuilder[] _nodesToBuild;
    }

    public class GenericMixedEntityDescriptor<T, U, V, W, X, Y> : EntityDescriptor
        where T : INodeBuilder, new()
        where U : INodeBuilder, new()
        where V : INodeBuilder, new()
        where W : INodeBuilder, new()
        where X : INodeBuilder, new()
        where Y : INodeBuilder, new()
    {
        static GenericMixedEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[]
            {
                new T(),
                new U(),
                new V(),
                new W(),
                new X(),
                new Y()
            };
        }
        public GenericMixedEntityDescriptor(params object[] componentsImplementor) :
            base(_nodesToBuild, componentsImplementor)
        { }

        static readonly INodeBuilder[] _nodesToBuild;
    }
}
