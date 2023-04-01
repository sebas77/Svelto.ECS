using Svelto.ECS.Hybrid;

namespace Svelto.ECS
{
    struct EntityInfoComponent: IManagedComponent
    {
        public IComponentBuilder[] componentsToBuild;
    }
}