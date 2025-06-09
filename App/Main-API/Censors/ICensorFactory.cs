using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Censors
{
    public interface ICensorFactory
    {
        T Censor<T>() where T : ICensor;
    }
}
