namespace Pawfect_API.Data.Entities.Types.Search
{
    public interface ISearchDataModel<T>
    {
        T ToSearchText();
    }
}
