namespace Pawfect_Notifications.Builders
{
    public interface IBuilderFactory
    {
        T Builder<T>() where T : IBuilder;
    }
}