namespace Svelto.ECS
{
    /// <summary>
    /// When implementing IEntityDescriptor directly the pattern to use is the following:
    ///
    ///  class DoofusEntityDescriptor: IEntityDescriptor
///    {
///    public IComponentBuilder[] componentsToBuild { get; } =
///    {
///        new ComponentBuilder<PositionEntityComponent>()
///      , new ComponentBuilder<DOTSEntityComponent>()
///      , new ComponentBuilder<VelocityEntityComponent>()
///      , new ComponentBuilder<SpeedEntityComponent>()
///      , ...
///    };
///    }
    /// </summary>
    public interface IEntityDescriptor
    {
        IComponentBuilder[] componentsToBuild { get; }
    }
}