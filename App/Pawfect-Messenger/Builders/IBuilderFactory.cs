namespace Pawfect_Messenger.Builders
{
    public interface IBuilderFactory
    {
        T Builder<T>() where T : IBuilder;
    }
}