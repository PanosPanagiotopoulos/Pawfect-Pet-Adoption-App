namespace Main_API.Builders
{
    public interface IBuilderFactory
    {
        T Builder<T>() where T : IBuilder;
    }
}