using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IUseMultipleResourceManagerImplementor: IImplementor
    {
         IECSManager[] resourceManager { set; }
    }
}