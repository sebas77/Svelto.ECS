namespace Svelto.Ticker
{
    public interface ILateTickable : ITickableBase
    {
        void LateTick(float deltaSec);
    }

    public interface IPhysicallyTickable : ITickableBase
    {
        void PhysicsTick(float deltaSec);
    }

    public interface ITickable : ITickableBase
    {
        void Tick(float deltaSec);
    }

    public interface IEndOfFrameTickable : ITickableBase
    {
        void EndOfFrameTick(float deltaSec);
    }

    public interface IIntervaledTickable : ITickableBase
    {
        void IntervaledTick();
    }

    public interface ITickableBase
    {
    }
}
