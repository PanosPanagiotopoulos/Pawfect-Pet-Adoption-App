using Main_API.Query.Queries;

namespace Main_API.Censors
{
    public interface ICensorFactory
    {
        T Censor<T>() where T : ICensor;
    }
}
