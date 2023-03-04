using Svelto.ECS.Hybrid;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    struct EntityInfoComponent: IManagedComponent
    {
        public IComponentBuilder[] componentsToBuild;
    }
}