using Pawfect_Notifications.Query.Queries;

namespace Pawfect_Notifications.Censors
{
    public interface ICensorFactory
    {
        T Censor<T>() where T : ICensor;
    }
}
