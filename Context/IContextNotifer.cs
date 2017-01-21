namespace Svelto.Context
{
    public interface IContextNotifer
    {
        void NotifyFrameworkInitialized();
        void NotifyFrameworkDeinitialized();

        void AddFrameworkInitializationListener(IWaitForFrameworkInitialization obj); 
        void AddFrameworkDestructionListener(IWaitForFrameworkDestruction obj);
    }
}
