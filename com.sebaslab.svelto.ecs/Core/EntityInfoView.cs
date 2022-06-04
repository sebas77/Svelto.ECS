namespace Svelto.ECS
{
    struct EntityInfoComponent: IBaseEntityComponent
    {
        public IComponentBuilder[] componentsToBuild;
    }
}