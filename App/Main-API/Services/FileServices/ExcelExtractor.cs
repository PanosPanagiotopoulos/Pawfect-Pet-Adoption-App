
using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Files;
using Main_API.Exceptions;
using Main_API.Models.Animal;
using Main_API.Models.AnimalType;
using Main_API.Models.Breed;
using Main_API.Query;
using Main_API.Query.Queries;
using Main_API.Services.AuthenticationServices;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace Pawfect_Pet_Adoption_App_API.Services.FileServices
{
    public class ExcelExtractor : IFileDataExtractor
    {
        private readonly FilesConfig _config;
        private readonly IQueryFactory _queryFactory;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;

        public ExcelExtractor
        (
            IQueryFactory queryFactory,
            IOptions<FilesConfig> options,
            IAuthorizationContentResolver authorizationContentResolver
        )
        {
            _config = options.Value;
            _queryFactory = queryFactory;
            _authorizationContentResolver = authorizationContentResolver;
        }

        public async Task<Byte[]> GenerateAnimalImportTemplate()
        {
            String shelterId = await _authorizationContentResolver.CurrentPrincipalShelter();
            if (String.IsNullOrEmpty(shelterId)) throw new ForbiddenException("Cannot Extract Animal data from excel if not shelter");

            ExcelPackage.License.SetNonCommercialPersonal("Pawfect");

            using ExcelPackage package = new ExcelPackage();

            ExcelWorksheet sheet = package.Workbook.Worksheets.Add("Animals");
            ExcelWorksheet refSheet = package.Workbook.Worksheets.Add(_config.ExcelExtractorConfig.ReferenceSheet.Name);

            List<String> headers = _config.ExcelExtractorConfig.Headers
                .Where(h => h != "AdoptionStatus")
                .ToList();

            for (int i = 0; i < headers.Count; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Locked = true;
                sheet.Column(i + 1).Width = 20;
            }

            sheet.Cells[_config.ExcelExtractorConfig.EditableRange].Style.Locked = false;

            foreach (KeyValuePair<String, List<String>> kvp in _config.ExcelExtractorConfig.StaticDropdowns)
            {
                if (kvp.Key == "AdoptionStatus") continue;

                int colIndex = headers.IndexOf(kvp.Key);
                if (colIndex == -1) continue;

                String colLetter = GetExcelColumnLetter(colIndex + 1);
                String address = $"{colLetter}2:{colLetter}{_config.ExcelExtractorConfig.MaxRows}";
                AddDropdown(sheet, address, kvp.Value);
            }

            List<(String Id, String Name)> breedData = await CollectBreedCollumnSetup();
            List<(String Id, String Name)> typeData = await CollectAnimalTypesCollumnSetup();

            for (int i = 0; i < typeData.Count; i++)
            {
                refSheet.Cells[i + 1, 1].Value = typeData[i].Name;
                refSheet.Cells[i + 1, 2].Value = typeData[i].Id;
            }

            for (int i = 0; i < breedData.Count; i++)
            {
                refSheet.Cells[i + 1, 3].Value = breedData[i].Name;
                refSheet.Cells[i + 1, 4].Value = breedData[i].Id;
            }

            String[] genderOptions = Enum.GetNames(typeof(Gender));
            for (int i = 0; i < genderOptions.Length; i++)
            {
                refSheet.Cells[i + 1, 5].Value = genderOptions[i];
            }

            if (_config.ExcelExtractorConfig.ReferenceSheet.Hidden)
            {
                refSheet.Hidden = eWorkSheetHidden.Hidden;
            }

            foreach (KeyValuePair<String, DynamicDropdownConfig> kvp in _config.ExcelExtractorConfig.DynamicDropdowns)
            {
                int colIndex = headers.IndexOf(kvp.Key);
                if (colIndex == -1) continue;

                String colLetter = GetExcelColumnLetter(colIndex + 1);
                String address = $"{colLetter}2:{colLetter}{_config.ExcelExtractorConfig.MaxRows}";

                int sourceRowCount = kvp.Key == "AnimalTypeName" ? typeData.Count : breedData.Count;
                Char sourceColumn = kvp.Key == "AnimalTypeName" ? 'A' : 'C';

                String formula = $"{kvp.Value.SourceSheet}!${sourceColumn}$1:${sourceColumn}${sourceRowCount}";
                AddDropdownFormula(sheet, address, formula);
            }

            if (_config.ExcelExtractorConfig.Protected)
            {
                sheet.Protection.IsProtected = true;
                if (!String.IsNullOrWhiteSpace(_config.ExcelExtractorConfig.ProtectionPassword))
                {
                    sheet.Protection.SetPassword(_config.ExcelExtractorConfig.ProtectionPassword);
                }
            }

            return package.GetAsByteArray();
        }

        private async Task<List<(String, String)>> CollectBreedCollumnSetup()
        {
            BreedQuery q = _queryFactory.Query<BreedQuery>();
            q.Offset = 0;
            q.PageSize = 100;
            q.Fields = new List<String> { nameof(Breed.Id), nameof(Breed.Name) };

            return (await q.CollectAsync()).Select(res => (res.Id, res.Name)).ToList();
        }

        private async Task<List<(String, String)>> CollectAnimalTypesCollumnSetup()
        {
            AnimalTypeQuery q = _queryFactory.Query<AnimalTypeQuery>();
            q.Offset = 0;
            q.PageSize = 100;
            q.Fields = new List<String> { nameof(AnimalType.Id), nameof(AnimalType.Name) };

            return (await q.CollectAsync()).Select(res => (res.Id, res.Name)).ToList();
        }

        private void AddDropdown(ExcelWorksheet sheet, String address, IEnumerable<String> options)
        {
            OfficeOpenXml.DataValidation.Contracts.IExcelDataValidationList validation = sheet.DataValidations.AddListValidation(address);
            foreach (var option in options)
                validation.Formula.Values.Add(option);
        }

        private void AddDropdownFormula(ExcelWorksheet sheet, String address, String formula)
        {
            OfficeOpenXml.DataValidation.Contracts.IExcelDataValidationList validation = sheet.DataValidations.AddListValidation(address);
            validation.Formula.ExcelFormula = formula;
        }

        private String GetExcelColumnLetter(int columnNumber)
        {
            int dividend = columnNumber;
            String columnName = String.Empty;

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }


        public async Task<List<AnimalPersist>> ExtractAnimalModelData(IFormFile modelsDataCsv)
        {
            String shelterId = await _authorizationContentResolver.CurrentPrincipalShelter();
            if (String.IsNullOrEmpty(shelterId)) throw new ForbiddenException("Cannot Extract Animal data from excel if not shelter");

            ExcelPackage.License.SetNonCommercialPersonal("Pawfect");

            using MemoryStream stream = new MemoryStream();
            await modelsDataCsv.CopyToAsync(stream);
            using ExcelPackage package = new ExcelPackage(stream);

            ExcelWorksheet sheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Animals");
            if (sheet == null) return new List<AnimalPersist>();

            int startRow = 2;
            int totalCols = sheet.Dimension.End.Column;
            int totalRows = sheet.Dimension.End.Row;

            // Build header mapping
            Dictionary<String, int> headerMap = new();
            for (int col = 1; col <= totalCols; col++)
            {
                String? header = sheet.Cells[1, col].Text?.Trim();
                if (!String.IsNullOrWhiteSpace(header))
                {
                    headerMap[header] = col;
                }
            }


            HashSet<String> breedNames = new();
            HashSet<String> animalTypeNames = new();

            List<(int RowIndex, AnimalPersist Animal, String? BreedName, String? TypeName)> stagedAnimals = new();

            for (int row = startRow; row <= totalRows; row++)
            {
                String? name = sheet.Cells[row, headerMap.GetValueOrDefault("Name")].Text;
                String? ageStr = sheet.Cells[row, headerMap.GetValueOrDefault("Age")].Text;
                String? genderStr = sheet.Cells[row, headerMap.GetValueOrDefault("Gender")].Text;
                String? description = sheet.Cells[row, headerMap.GetValueOrDefault("Description")].Text;
                String? weightStr = sheet.Cells[row, headerMap.GetValueOrDefault("Weight")].Text;
                String? healthStatus = sheet.Cells[row, headerMap.GetValueOrDefault("HealthStatus")].Text;
                String? breedName = sheet.Cells[row, headerMap.GetValueOrDefault("BreedName")].Text;
                String? typeName = sheet.Cells[row, headerMap.GetValueOrDefault("AnimalTypeName")].Text;

                double.TryParse(ageStr, out double age);
                double.TryParse(weightStr, out double weight);
                Enum.TryParse(genderStr, ignoreCase: true, out Gender gender);

                if (!String.IsNullOrWhiteSpace(breedName))
                    breedNames.Add(breedName);

                if (!String.IsNullOrWhiteSpace(typeName))
                    animalTypeNames.Add(typeName);

                AnimalPersist animal = new AnimalPersist
                {
                    Id = null,
                    Name = name,
                    Age = age,
                    Gender = gender,
                    Description = description,
                    Weight = weight,
                    HealthStatus = healthStatus,
                    ShelterId = shelterId,
                    BreedId = null,
                    AnimalTypeId = null,
                    AttachedPhotosIds = null,
                    AdoptionStatus = AdoptionStatus.Available
                };

                stagedAnimals.Add((row, animal, breedName, typeName));
            }

            // Collect ID mappings only for used names
            Dictionary<String, String> breedMap = await CollectBreedNameToIdMap(breedNames.ToList());
            Dictionary<String, String> typeMap = await CollectAnimalTypeNameToIdMap(animalTypeNames.ToList());

            // Apply mappings to staged animal objects
            foreach ((int rowIndex, AnimalPersist animal, String breedName, String typeName) in stagedAnimals)
            {
                if (!String.IsNullOrWhiteSpace(breedName) && breedMap.TryGetValue(breedName, out String? breedId))
                {
                    animal.BreedId = breedId;
                }

                if (!String.IsNullOrWhiteSpace(typeName) && typeMap.TryGetValue(typeName, out String? typeId))
                {
                    animal.AnimalTypeId = typeId;
                }
            }

            return stagedAnimals.Select(t => t.Animal).ToList();
        }

        private async Task<Dictionary<String, String>> CollectBreedNameToIdMap(List<String> breedNames)
        {
            BreedQuery q = _queryFactory.Query<BreedQuery>();
            q.Offset = 0;
            q.PageSize = 100;
            q.Names = breedNames;
            q.Fields = new List<String> { nameof(Breed.Id), nameof(Breed.Name) };

            return (await q.CollectAsync()).ToDictionary(x => x.Name, x => x.Id);
        }

        private async Task<Dictionary<String, String>> CollectAnimalTypeNameToIdMap(List<String> animalTypeNames)
        {
            AnimalTypeQuery q = _queryFactory.Query<AnimalTypeQuery>();
            q.Offset = 0;
            q.PageSize = 100;
            q.Names = animalTypeNames;
            q.Fields = new List<String> { nameof(AnimalType.Id), nameof(AnimalType.Name) };

            return (await q.CollectAsync()).ToDictionary(x => x.Name, x => x.Id);
        }
    }
}
