namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public interface IBuilderFactory
    {
        T Builder<T>() where T : IBuilder;
    }
}