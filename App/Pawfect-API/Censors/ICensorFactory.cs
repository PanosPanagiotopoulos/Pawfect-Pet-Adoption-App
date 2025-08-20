using Pawfect_API.Query.Queries;

namespace Pawfect_API.Censors
{
    public interface ICensorFactory
    {
        T Censor<T>() where T : ICensor;
    }
}
