namespace Svelto.Ticker
{
    public interface ITicker
    {
        void Add(ITickableBase tickable);
        void Remove(ITickableBase tickable);
    }
}
