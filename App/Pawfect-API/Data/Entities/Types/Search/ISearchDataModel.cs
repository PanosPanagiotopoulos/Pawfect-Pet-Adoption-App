namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Search
{
    public interface ISearchDataModel<T>
    {
        T ToSearchText();
    }
}
