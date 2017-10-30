#if UNITY_5 || UNITY_5_3_OR_NEWER
namespace Svelto.Context
{
    public interface ICompositionRoot
    {
        void OnContextCreated(UnityContext contextHolder);
        void OnContextInitialized();
        void OnContextDestroyed();
    }
}
#endif