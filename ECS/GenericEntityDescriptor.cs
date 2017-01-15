namespace Svelto.ECS
{
    class GenericEntityDescriptor<T> : EntityDescriptor
        where T : NodeWithID, new()
    {
        static GenericEntityDescriptor()
        {
            _nodesToBuild = new INodeBuilder[]
            {
                new NodeBuilder<T>()
            };
        }
        public GenericEntityDescriptor(params object[] componentsImplementor) : base(_nodesToBuild, componentsImplementor)
        {
        }
        static INodeBuilder[] _nodesToBuild;
    }

    class GenericEntityDescriptor<T, U> : EntityDescriptor
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
        {
        }
        static INodeBuilder[] _nodesToBuild;
    }

    class GenericEntityDescriptor<T, U, V> : EntityDescriptor
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
        {
        }
        static INodeBuilder[] _nodesToBuild;
    }

    class GenericEntityDescriptor<T, U, V, W> : EntityDescriptor
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
        {
        }
        static INodeBuilder[] _nodesToBuild;
    }

    class GenericEntityDescriptor<T, U, V, W, X> : EntityDescriptor
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
        {
        }
        static INodeBuilder[] _nodesToBuild;
    }
    class GenericEntityDescriptor<T, U, V, W, X, Y> : EntityDescriptor
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
        {
        }
        static INodeBuilder[] _nodesToBuild;
    }
}
