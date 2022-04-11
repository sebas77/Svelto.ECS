using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IUseResourceManagerImplementor: IImplementor
    {
         IECSManager resourceManager { set; }
    }
}