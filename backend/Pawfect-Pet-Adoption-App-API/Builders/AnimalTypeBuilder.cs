using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
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
            CreateMap<AnimalType, AnimalType>();

            // POST Request Dto Models
            CreateMap<AnimalType, AnimalTypePersist>();
            CreateMap<AnimalTypePersist, AnimalType>();
        }

    }

    public class AnimalTypeBuilder : BaseBuilder<AnimalTypeDto, AnimalType>
    {
        // Ορίστε τις παραμέτρους αναζήτησης για τον κατασκευαστή
        public override BaseBuilder<AnimalTypeDto, AnimalType> SetLookup(Lookup lookup) { base.LookupParams = lookup; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<AnimalTypeDto>> BuildDto(List<AnimalType> entities, List<String> fields)
        {
            // Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
            (List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

            List<AnimalTypeDto> result = new List<AnimalTypeDto>();
            foreach (AnimalType e in entities)
            {
                AnimalTypeDto dto = new AnimalTypeDto();
                dto.Id = e.Id;
                if (nativeFields.Contains(nameof(AnimalTypeDto.Name))) dto.Name = e.Name;
                if (nativeFields.Contains(nameof(AnimalTypeDto.Description))) dto.Description = e.Description;
                if (nativeFields.Contains(nameof(AnimalTypeDto.CreatedAt))) dto.CreatedAt = e.CreatedAt;
                if (nativeFields.Contains(nameof(AnimalTypeDto.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;

                result.Add(dto);
            }

            return await Task.FromResult(result);
        }
    }
}