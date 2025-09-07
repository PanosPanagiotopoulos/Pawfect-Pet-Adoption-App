using Pawfect_Messenger.Query.Queries;

namespace Pawfect_Messenger.Censors
{
    public interface ICensorFactory
    {
        T Censor<T>() where T : ICensor;
    }
}
