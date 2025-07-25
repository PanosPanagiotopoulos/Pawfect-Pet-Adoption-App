﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Main_API.Builders;
using Main_API.Censors;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models.Animal;
using Main_API.Models.Lookups;
using Main_API.Query;
using Main_API.Services.AnimalServices;
using Main_API.Transactions;
using Pawfect_Pet_Adoption_App_API.Query;
using Main_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Services.FileServices;

namespace Main_API.Controllers
{
	[ApiController]
	[Route("api/animals")]
	public class AnimalController : ControllerBase
	{
        private readonly IAnimalService _animalService;
        private readonly ILogger<AnimalController> _logger;
        private readonly IBuilderFactory _builderFactory;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly Lazy<IFileDataExtractor> _excelExtractor;
        private readonly IQueryFactory _queryFactory;

        public AnimalController
        (
            IAnimalService animalService,
            ILogger<AnimalController> logger,
            IBuilderFactory builderFactory,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            Lazy<IFileDataExtractor> excelExtractor,
            IQueryFactory queryFactory
        )
        {
            _animalService = animalService;
            _logger = logger;
            _builderFactory = builderFactory;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _excelExtractor = excelExtractor;
            _queryFactory = queryFactory;
        }

        /// <summary>
        /// Query ζώων.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
        /// </summary>
        [HttpPost("query")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryAnimals([FromBody] AnimalLookup animalLookup)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AuthContext context = _contextBuilder.OwnedFrom(animalLookup).AffiliatedWith(animalLookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AnimalCensor>().Censor([.. animalLookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying animals");

            animalLookup.Fields = censoredFields;
            AnimalQuery q = animalLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation);
            
            List<Data.Entities.Animal> datas = await q.CollectAsync();

            List<Animal> models = await _builderFactory.Builder<AnimalBuilder>()
                .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(datas, [..animalLookup.Fields]);

            if (models == null) throw new NotFoundException("Animals not found", JsonHelper.SerializeObjectFormatted(animalLookup), typeof(Data.Entities.Animal));



            return Ok(new QueryResult<Animal>()
            {
                Items = models,
                Count = await q.CountAsync()
            });
        }

        [HttpPost("query/free-view")]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> QueryAnimalsFreeView([FromBody] AnimalLookup animalLookup)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            animalLookup.Fields = _censorFactory.Censor<AnimalCensor>().PseudoCensor([.. animalLookup.Fields]);

            AnimalQuery q = animalLookup.EnrichLookup(_queryFactory);

            List<Data.Entities.Animal> datas = await q.CollectAsync();

            List<Animal> models = await _builderFactory.Builder<AnimalBuilder>()
                .Build(datas, [.. animalLookup.Fields]);

            if (models == null) throw new NotFoundException("Animals not found", JsonHelper.SerializeObjectFormatted(animalLookup), typeof(Data.Entities.Animal));

            return Ok(new QueryResult<Animal>()
            {
                Items = models,
                Count = await q.CountAsync()
            });
        }

        /// <summary>
        /// Λήψη ζώου με βάση το ID.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 500 String
        /// </summary>
        [HttpGet("{id}")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetAnimal(String id, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            AnimalLookup lookup = new AnimalLookup
            {
                Offset = 1,
                PageSize = 1,
                Ids = new List<String> { id },
                Fields = fields
            };

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<AnimalCensor>().Censor(BaseCensor.PrepareFieldsList([.. lookup.Fields]), context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying animals");

            lookup.Fields = censoredFields;
            Animal model = (await _builderFactory.Builder<AnimalBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
            .FirstOrDefault();

            if (model == null) throw new NotFoundException("Animal not found", JsonHelper.SerializeObjectFormatted(lookup), typeof(Data.Entities.Animal));

            return Ok(model);
		}

		/// <summary>
		/// Persist an animal.
		/// </summary>
		[HttpPost("persist")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Persist([FromBody] AnimalPersist model, [FromQuery] List<String> fields)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

            fields = BaseCensor.PrepareFieldsList(fields);

            Animal animal = await _animalService.Persist(model, fields);

			return Ok(animal);
		}

        [HttpGet("import-template/excel")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> GetAnimalTempalteExcel()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            Byte[] template = await _excelExtractor.Value.GenerateAnimalImportTemplate();

            FileContentResult result = new FileContentResult(template, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = "import_template.xlsx"
            };

            return result;
        }

        [HttpPost("from-template/excel")]
        [Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> ImportFromExcelTemplate()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            IFormFileCollection files = Request.Form.Files;
            if (files == null || files.Count != 1) return BadRequest("Invalid amount of files provided");

            IFormFile excelFile = files.FirstOrDefault();
            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)) return BadRequest("Not excel file provided");

            List<AnimalPersist> exctractedModels = await this._excelExtractor.Value.ExtractAnimalModelData(excelFile);

            return Ok(exctractedModels);
        }

        /// <summary>
        /// Delete an animal by ID.
        /// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
        /// </summary>
        [HttpPost("delete")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> Delete([FromBody] String id)
		{
			if (String.IsNullOrEmpty(id) || !ModelState.IsValid) return BadRequest(ModelState);

			await _animalService.Delete(id);

			return Ok();
		}

		/// <summary>
		/// Delete multiple animals by IDs.
		/// Επιστρέφει: 200 OK, 400 ValidationProblemDetails, 404 NotFound, 500 String
		/// </summary>
		[HttpPost("delete/many")]
		[Authorize]
        [ServiceFilter(typeof(MongoTransactionFilter))]
        public async Task<IActionResult> DeleteMany([FromBody] List<String> ids)
		{
			if (ids == null || !ids.Any() || !ModelState.IsValid) return BadRequest(ModelState);

			await _animalService.Delete(ids);

			return Ok();
		}
	}
}
