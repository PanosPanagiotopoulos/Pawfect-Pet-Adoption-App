using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class AutoAnimalTypeBuilder : Profile
    {
        // Builders for object conversions from Entities to Models and vice versa
        // Builder for Entity: AnimalType
        public AutoAnimalTypeBuilder()
        {
            // Αντιστοίχιση για το Entity: AnimalType στο AnimalType για αντιγραφή αντικειμένου
            CreateMap<Data.Entities.AnimalType, Data.Entities.AnimalType>();

            // POST Request Dto Models
            CreateMap<Data.Entities.AnimalType, AnimalTypePersist>();
            CreateMap<AnimalTypePersist, Data.Entities.AnimalType>();
        }

    }

    public class AnimalTypeBuilder : BaseBuilder<Models.AnimalType.AnimalType, Data.Entities.AnimalType>
    {
        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        public AnimalTypeBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<Models.AnimalType.AnimalType>> Build(List<Data.Entities.AnimalType> entities, List<String> fields)
        {
            // Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
            (List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

            List<Models.AnimalType.AnimalType> result = new List<Models.AnimalType.AnimalType>();
            foreach (Data.Entities.AnimalType e in entities)
            {
                Models.AnimalType.AnimalType dto = new Models.AnimalType.AnimalType();
                dto.Id = e.Id;
                if (nativeFields.Contains(nameof(Models.AnimalType.AnimalType.Name))) dto.Name = e.Name;
                if (nativeFields.Contains(nameof(Models.AnimalType.AnimalType.Description))) dto.Description = e.Description;
                if (nativeFields.Contains(nameof(Models.AnimalType.AnimalType.CreatedAt))) dto.CreatedAt = e.CreatedAt;
                if (nativeFields.Contains(nameof(Models.AnimalType.AnimalType.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;

                result.Add(dto);
            }

            return await Task.FromResult(result);
        }
    }
}