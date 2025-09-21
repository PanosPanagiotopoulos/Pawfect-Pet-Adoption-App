using Microsoft.Extensions.Options;
using Pawfect_API.Builders;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Models.Animal;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Query;
using Pawfect_API.Data.Entities.Types.AiAssistant;
using Pawfect_API.Models.AiAssistant;
using Pawfect_API.Data.Entities.Types.AiContext;
using Pawfect_API.DevTools;
using System.Text.RegularExpressions;
using Mistral.SDK;
using Pawfect_API.Exceptions;

namespace Pawfect_API.Services.AiAssistantServices
{
    public class AiAssistantService : IAiAssistantService
    {
        private readonly ILogger<AiAssistantService> _logger;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly AiAssistantConfig _aiAssistantConfig;

        public AiAssistantService
        (
            ILogger<AiAssistantService> logger,
            IOptions<AiAssistantConfig> aiAssistantOptions,
            IQueryFactory queryFactory,
            IBuilderFactory builderFactory
        )
        {
            this._logger = logger;
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
            this._aiAssistantConfig = aiAssistantOptions.Value;
        }
        public async Task<CompletionsResponse> CompleteionAsync(CompletionsRequest request)
        {
            String prompt = this.CleanPrompt(request.Prompt);

            AnimalLookup ragLookup = new AnimalLookup();
            AnimalLookup focusedAnimalLookup = null;

            ragLookup.Query = prompt;
            ragLookup.UseVectorSearch = true;
            ragLookup.Offset = 0;
            ragLookup.PageSize = _aiAssistantConfig.RagBatchSize;
            ragLookup.Fields = _aiAssistantConfig.ContextFields;

            Boolean includesFocusedAnimal = !String.IsNullOrWhiteSpace(request.ContextAnimalId);
            if (includesFocusedAnimal)
            {
                focusedAnimalLookup = new AnimalLookup();
                focusedAnimalLookup.Ids = [request.ContextAnimalId];
                focusedAnimalLookup.Offset = 0;
                focusedAnimalLookup.PageSize = 1;
                focusedAnimalLookup.Fields = _aiAssistantConfig.ContextFields;
            }

            List<Data.Entities.Animal>[] contextData = await Task.WhenAll(
                ragLookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(),
                focusedAnimalLookup?.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync() ?? Task.FromResult(new List<Data.Entities.Animal>())
            );

            if (includesFocusedAnimal && contextData[1] == null || contextData[1].Count == 0)
                throw new NotFoundException("Focused animal was not found");

            List<Animal> ragContext = await _builderFactory.Builder<AnimalBuilder>()
            .Build(
                contextData[0].Concat(contextData[1]).ToList(),
                _aiAssistantConfig.ContextFields
            );

            List<AnimalContext> animalContexts = AnimalContext.FromAnimalModels(ragContext, includesFocusedAnimal ? [request.ContextAnimalId] : null);

            String instructions = await System.IO.File.ReadAllTextAsync(_aiAssistantConfig.InstructionsPath);
            instructions = instructions.Replace(_aiAssistantConfig.RagContextPlaceholder, JsonHelper.SerializeObjectFormattedSafe(animalContexts));

            using MistralClient client = new MistralClient(_aiAssistantConfig.ApiKey);

            Mistral.SDK.DTOs.ChatCompletionRequest chatRequest = new Mistral.SDK.DTOs.ChatCompletionRequest()
            {
                Model = ModelDefinitions.OpenMixtral8x7b,
                Messages = new List<Mistral.SDK.DTOs.ChatMessage>()
                {
                    new Mistral.SDK.DTOs.ChatMessage()
                    {
                        Role = Mistral.SDK.DTOs.ChatMessage.RoleEnum.System,
                        Content = instructions
                    },
                    new Mistral.SDK.DTOs.ChatMessage()
                    {
                        Role = Mistral.SDK.DTOs.ChatMessage.RoleEnum.User,
                        Content = prompt
                    }
                },
                Temperature = (Decimal) 0.5,
                TopP = (Decimal) 0.5,
                MaxTokens = 3000,
                Stream =  false
            };

            Mistral.SDK.DTOs.ChatCompletionResponse response = await client.Completions.GetCompletionAsync(chatRequest);

            if (response == null || response.Choices == null || response.Choices.Count == 0)
            {
                _logger.LogError("AI Assistant: No response from Mistral API.");
                throw new Exception("No response from AI service.");
            }

            return new CompletionsResponse()
            {
                 Response = response.Choices.FirstOrDefault()?.Message?.Content ?? "No response"
            };
        }

        private String CleanPrompt(String prompt) => Regex.Replace(Regex.Replace(prompt ?? "", @"[^\w\s]", "").ToLowerInvariant().Trim(), @"\s+", " ").Trim();

    }
}
