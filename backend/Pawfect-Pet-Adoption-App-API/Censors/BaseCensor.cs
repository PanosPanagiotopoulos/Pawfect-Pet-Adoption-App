using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using System.Linq;

namespace Pawfect_Pet_Adoption_App_API.Censors
{
    public interface ICensor 
    {
        Task<List<String>> Censor(List<String> fields, AuthContext context);
    }
    public abstract class BaseCensor : ICensor
    {
        public abstract Task<List<String>> Censor(List<String> fields, AuthContext context);
        
        protected List<String> ExtractNonPrefixed(List<String> fields)
        {
            if (fields == null) return new List<String>();
            
            List<String> baseRootFields = [..fields.Where(field => !field.Contains('.'))];
            baseRootFields = [.. baseRootFields?.Distinct()];
            
            return  baseRootFields ?? new List<String>();
        }

        protected List<String> ExtractPrefixed(List<String> fields, String root)
        {
            if (fields == null || String.IsNullOrEmpty(root)) return new List<String>();

            List<String> prefixed = [.. fields.Where(field => field.StartsWith(root) && field.Contains('.'))];
            List<String> baseRootFields = [.. prefixed.Select(field => field.Substring(root.Length + 1)).Distinct()];

            return baseRootFields ?? new List<String>();
        }

        protected List<String> ExtractForeign(List<String> fields, Type entityType)
        {
            List<String> foreignFields = [.. fields.Concat(EntityHelper.GetAllForeignPropertyNames(entityType))];
            return foreignFields ?? new List<String>(); 
        }

        protected List<String> AsPrefixed(List<String> nonPrefixed , String prefix) =>[..nonPrefixed.Select(str => $"{prefix}.{str}")];

        public static List<String> PrepareFieldsList(List<String> fieldList)
        {
            if (fieldList == null || fieldList.Count == 0) return new List<String>();
            return [..
                fieldList.Select(field =>
                {
                    return String.Join(".", field.Split(".").Select(part => char.ToUpper(part[0]) + part.Substring(1)));
                })
            ];
        }
    }
}
