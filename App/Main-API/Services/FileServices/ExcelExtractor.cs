using Main_API.Data.Entities;
using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Files;
using Main_API.Exceptions;
using Main_API.Models.Animal;
using Main_API.Query;
using Main_API.Query.Queries;
using Main_API.Services.AuthenticationServices;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using OfficeOpenXml.DataValidation;
using OfficeOpenXml.DataValidation.Contracts;

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
            if (String.IsNullOrEmpty(shelterId)) throw new ForbiddenException("Cannot generate animal import template if not shelter");

            ExcelPackage.License.SetNonCommercialPersonal("Pawfect");

            using ExcelPackage package = new ExcelPackage();
            ExcelWorksheet sheet = package.Workbook.Worksheets.Add("Animals");
            ExcelWorksheet refSheet = package.Workbook.Worksheets.Add(_config.ExcelExtractorConfig.ReferenceSheet.Name);

            // Define headers based on configuration, excluding AdoptionStatus
            List<String> headers = _config.ExcelExtractorConfig.Headers
                .Where(h => h != nameof(AnimalPersist.AdoptionStatus))
                .ToList();

            // Set headers and lock them
            for (int i = 0; i < headers.Count; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Locked = true;
                sheet.Column(i + 1).Width = 20;
            }

            // Unlock editable range
            sheet.Cells[_config.ExcelExtractorConfig.EditableRange].Style.Locked = false;

            // Fetch data for dropdowns
            List<(String Id, String Name)> typeData = await CollectAnimalTypesColumnSetup();
            List<(String Id, String Name, String AnimalTypeId)> breedData = await CollectBreedColumnSetup();

            Dictionary<String, String> typeMap = typeData.ToDictionary(t => t.Id, t => t.Name);

            // Populate reference sheet
            // Column 1: AnimalTypeName, Column 2: AnimalTypeId
            for (int i = 0; i < typeData.Count; i++)
            {
                refSheet.Cells[i + 1, 1].Value = typeData[i].Name;
                refSheet.Cells[i + 1, 2].Value = typeData[i].Id;
            }

            // Column 3: BreedName (formatted as AnimalTypeName: BreedName), Column 4: BreedId
            for (int i = 0; i < breedData.Count; i++)
            {
                String animalTypeName = typeMap.TryGetValue(breedData[i].AnimalTypeId, out String name) ? name : "Unknown";
                refSheet.Cells[i + 1, 3].Value = $"{animalTypeName}: {breedData[i].Name}";
                refSheet.Cells[i + 1, 4].Value = breedData[i].Id;
            }

            // Column 5: Gender options (dynamically retrieved from Gender enum)
            String[] genderOptions = Enum.GetNames(typeof(Gender));
            for (int i = 0; i < genderOptions.Length; i++)
            {
                refSheet.Cells[i + 1, 5].Value = genderOptions[i];
            }

            // Hide reference sheet if configured
            if (_config.ExcelExtractorConfig.ReferenceSheet.Hidden)
            {
                refSheet.Hidden = eWorkSheetHidden.Hidden;
            }

            // Apply validations to columns
            foreach (String header in headers)
            {
                int colIndex = headers.IndexOf(header);
                String colLetter = GetExcelColumnLetter(colIndex + 1);
                String range = $"{colLetter}2:{colLetter}{_config.ExcelExtractorConfig.MaxRows + 1}";

                if (String.Equals(header, nameof(AnimalPersist.Name), StringComparison.OrdinalIgnoreCase))
                {
                    // Minimum length 2, required
                    AddTextLengthValidation(sheet, range, 2, Int32.MaxValue, "The animal's name must have at least 2 characters.", false);
                }
                else if (String.Equals(header, nameof(AnimalPersist.Age), StringComparison.OrdinalIgnoreCase))
                {
                    // Between 0.1 and 40, required
                    AddDecimalValidation(sheet, range, 0.1, 40, "Age must be between 0.1 and 40 years.", false);
                }
                else if (String.Equals(header, nameof(AnimalPersist.Gender), StringComparison.OrdinalIgnoreCase))
                {
                    // Dropdown for Gender enum values, required
                    String genderFormula = $"{_config.ExcelExtractorConfig.ReferenceSheet.Name}!$E$1:$E${genderOptions.Length}";
                    AddDropdownFormula(sheet, range, genderFormula, "Invalid animal gender. Choose a valid gender from the dropdown.", false);
                }
                else if (String.Equals(header, nameof(AnimalPersist.Description), StringComparison.OrdinalIgnoreCase))
                {
                    // Minimum length 10, required
                    AddTextLengthValidation(sheet, range, 10, Int32.MaxValue, "The animal's description must have at least 10 characters.", false);
                }
                else if (String.Equals(header, nameof(AnimalPersist.Weight), StringComparison.OrdinalIgnoreCase))
                {
                    // Between 0.1 and 500, required
                    AddDecimalValidation(sheet, range, 0.1, 500, "Weight must be between 0.1 and 500 kilograms.", false);
                }
                else if (String.Equals(header, nameof(AnimalPersist.HealthStatus), StringComparison.OrdinalIgnoreCase))
                {
                    // Minimum length 8, required
                    AddTextLengthValidation(sheet, range, 8, Int32.MaxValue, "Health status must have at least 8 characters.", false);
                }
                else if (String.Equals(header, String.Join(' ', nameof(Main_API.Models.Animal.Animal.AnimalType), nameof(Main_API.Models.AnimalType.AnimalType.Name)), StringComparison.OrdinalIgnoreCase))
                {
                    // Dropdown for animal types, required
                    String typeFormula = $"{_config.ExcelExtractorConfig.ReferenceSheet.Name}!$A$1:$A${typeData.Count}";
                    AddDropdownFormula(sheet, range, typeFormula, "Select a valid animal type from the dropdown.", false);
                }
                else if (String.Equals(header, String.Join(' ', nameof(Main_API.Models.Animal.Animal.Breed), nameof(Main_API.Models.Breed.Breed.Name)), StringComparison.OrdinalIgnoreCase))
                {
                    // Dropdown for breeds, required
                    String breedFormula = $"{_config.ExcelExtractorConfig.ReferenceSheet.Name}!$C$1:$C${breedData.Count}";
                    AddDropdownFormula(sheet, range, breedFormula, "Select a valid breed from the dropdown.", false);
                }
            }

            // Protect sheet if configured
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

        private async Task<List<(String Id, String Name, String AnimalTypeId)>> CollectBreedColumnSetup()
        {
            BreedQuery q = _queryFactory.Query<BreedQuery>();
            q.Offset = 0;
            q.PageSize = 1000;
            q.Fields = q.FieldNamesOf(new List<String> { nameof(Main_API.Models.Breed.Breed.Id), nameof(Main_API.Models.Breed.Breed.Name), $"{nameof(Main_API.Models.Breed.Breed.AnimalType)}.{nameof(Main_API.Models.AnimalType.AnimalType.Id)}" });

            List<Main_API.Data.Entities.Breed> breeds = await q.CollectAsync();
            return breeds.Select(res => (res.Id, res.Name, res.AnimalTypeId)).ToList();
        }

        private async Task<List<(String Id, String Name)>> CollectAnimalTypesColumnSetup()
        {
            AnimalTypeQuery q = _queryFactory.Query<AnimalTypeQuery>();
            q.Offset = 0;
            q.PageSize = 1000;
            q.Fields = q.FieldNamesOf(new List<String> { nameof(Main_API.Models.AnimalType.AnimalType.Id), nameof(Main_API.Models.AnimalType.AnimalType.Name) });

            List<Main_API.Data.Entities.AnimalType> types = await q.CollectAsync();
            return types.Select(res => (res.Id, res.Name)).ToList();
        }


        private void AddDropdownFormula(ExcelWorksheet sheet, String address, String formula, String errorMessage, Boolean allowBlank)
        {
            IExcelDataValidationList validation = sheet.DataValidations.AddListValidation(address);
            validation.Formula.ExcelFormula = formula;
            validation.ShowErrorMessage = true;
            validation.Error = errorMessage;
            validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            validation.AllowBlank = allowBlank;
        }

        private void AddDecimalValidation(ExcelWorksheet sheet, String address, Double min, Double max, String errorMessage, Boolean allowBlank)
        {
            IExcelDataValidationDecimal validation = sheet.DataValidations.AddDecimalValidation(address);
            validation.Formula.Value = min;
            validation.Formula2.Value = max;
            validation.Operator = ExcelDataValidationOperator.between;
            validation.ShowErrorMessage = true;
            validation.Error = errorMessage;
            validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            validation.AllowBlank = allowBlank;
        }

        private void AddTextLengthValidation(ExcelWorksheet sheet, String address, int minLength, int maxLength, String errorMessage, Boolean allowBlank)
        {
            IExcelDataValidationInt validation = sheet.DataValidations.AddTextLengthValidation(address);
            validation.Formula.Value = minLength;
            validation.Formula2.Value = maxLength;
            validation.Operator = ExcelDataValidationOperator.between;
            validation.ShowErrorMessage = true;
            validation.Error = errorMessage;
            validation.ErrorStyle = ExcelDataValidationWarningStyle.stop;
            validation.AllowBlank = allowBlank;
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
            ExcelPackage.License.SetNonCommercialPersonal("Pawfect");

            using MemoryStream stream = new MemoryStream();
            await modelsDataCsv.CopyToAsync(stream);
            using ExcelPackage package = new ExcelPackage(stream);

            ExcelWorksheet sheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Animals");
            if (sheet == null) return new List<AnimalPersist>();

            ExcelWorksheet refSheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == _config.ExcelExtractorConfig.ReferenceSheet.Name);
            if (refSheet == null) throw new Exception("Reference sheet not found");

            // Build reference data maps
            Dictionary<String, String> animalTypeMap = new Dictionary<String, String>();
            Dictionary<String, String> breedMap = new Dictionary<String, String>();
            HashSet<String> genderOptions = new HashSet<String>(StringComparer.OrdinalIgnoreCase);

            // Populate animalTypeMap: typeName -> typeId
            int row = 1;
            while (!String.IsNullOrWhiteSpace(refSheet.Cells[row, 1].Text))
            {
                String typeName = refSheet.Cells[row, 1].Text?.Trim();
                String typeId = refSheet.Cells[row, 2].Text?.Trim();
                if (!String.IsNullOrWhiteSpace(typeName) && !String.IsNullOrWhiteSpace(typeId))
                {
                    animalTypeMap[typeName] = typeId;
                }
                row++;
            }

            // Populate breedMap: formattedBreedName -> breedId
            row = 1;
            while (!String.IsNullOrWhiteSpace(refSheet.Cells[row, 3].Text))
            {
                String breedName = refSheet.Cells[row, 3].Text?.Trim();
                String breedId = refSheet.Cells[row, 4].Text?.Trim();
                if (!String.IsNullOrWhiteSpace(breedName) && !String.IsNullOrWhiteSpace(breedId))
                {
                    breedMap[breedName] = breedId;
                }
                row++;
            }

            // Populate genderOptions
            row = 1;
            while (!String.IsNullOrWhiteSpace(refSheet.Cells[row, 5].Text))
            {
                String gender = refSheet.Cells[row, 5].Text?.Trim();
                if (!String.IsNullOrWhiteSpace(gender))
                {
                    genderOptions.Add(gender);
                }
                row++;
            }

            int startRow = 2;
            int totalCols = sheet.Dimension.End.Column;
            int totalRows = sheet.Dimension.End.Row;

            Dictionary<String, int> headerMap = new Dictionary<String, int>();
            for (int col = 1; col <= totalCols; col++)
            {
                String header = sheet.Cells[1, col].Text?.Trim();
                if (!String.IsNullOrWhiteSpace(header))
                {
                    headerMap[header] = col;
                }
            }

            List<AnimalPersist> animals = new List<AnimalPersist>();

            for (int rowIdx = startRow; rowIdx <= totalRows; rowIdx++)
            {
                String name = sheet.Cells[rowIdx, headerMap.GetValueOrDefault(nameof(AnimalPersist.Name))].Text;
                String ageStr = sheet.Cells[rowIdx, headerMap.GetValueOrDefault(nameof(AnimalPersist.Age))].Text;
                String genderStr = sheet.Cells[rowIdx, headerMap.GetValueOrDefault(nameof(AnimalPersist.Gender))].Text;
                String description = sheet.Cells[rowIdx, headerMap.GetValueOrDefault(nameof(AnimalPersist.Description))].Text;
                String weightStr = sheet.Cells[rowIdx, headerMap.GetValueOrDefault(nameof(AnimalPersist.Weight))].Text;
                String healthStatus = sheet.Cells[rowIdx, headerMap.GetValueOrDefault(nameof(AnimalPersist.HealthStatus))].Text;
                String breedName = sheet.Cells[rowIdx, headerMap.GetValueOrDefault(String.Join(' ', nameof(Main_API.Models.Animal.Animal.Breed), nameof(Main_API.Models.Breed.Breed.Name)))].Text;
                String typeName = sheet.Cells[rowIdx, headerMap.GetValueOrDefault(String.Join(' ', nameof(Main_API.Models.Animal.Animal.AnimalType), nameof(Main_API.Models.AnimalType.AnimalType.Name)))].Text;

                // Check for completeness
                if (String.IsNullOrWhiteSpace(name) ||
                    String.IsNullOrWhiteSpace(ageStr) || !Double.TryParse(ageStr, out Double age) || age < 0.1 || age > 40 ||
                    String.IsNullOrWhiteSpace(genderStr) || !genderOptions.Contains(genderStr) ||
                    String.IsNullOrWhiteSpace(description) || description.Length < 10 ||
                    String.IsNullOrWhiteSpace(weightStr) || !Double.TryParse(weightStr, out Double weight) || weight < 0.1 || weight > 500 ||
                    String.IsNullOrWhiteSpace(healthStatus) || healthStatus.Length < 8 ||
                    String.IsNullOrWhiteSpace(typeName) || !animalTypeMap.ContainsKey(typeName) ||
                    String.IsNullOrWhiteSpace(breedName) || !breedMap.ContainsKey(breedName))
                {
                    continue; 
                }

                Double.TryParse(ageStr, out  age);
                Double.TryParse(weightStr, out weight);

                Gender gender = Gender.Male; 
                if (genderOptions.Contains(genderStr))
                {
                    Enum.TryParse(genderStr, true, out gender);
                }

                AnimalPersist animal = new AnimalPersist
                {
                    Id = null,
                    Name = name,
                    Age = age,
                    Gender = gender,
                    Description = description,
                    Weight = weight,
                    HealthStatus = healthStatus,
                    BreedId = breedMap.ContainsKey(breedName) ? breedMap[breedName] : null,
                    AnimalTypeId = animalTypeMap.ContainsKey(typeName) ? animalTypeMap[typeName] : null,
                    AdoptionStatus = AdoptionStatus.Available
                };

                animals.Add(animal);
            }

            return animals;
        }
    }
}