namespace Svelto.ECS
{
    struct EntityInfoComponentView: IEntityComponent
    {
        public IComponentBuilder[] componentsToBuild;
    }
}