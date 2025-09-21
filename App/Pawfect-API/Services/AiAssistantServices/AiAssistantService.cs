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
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.AI;

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

            if (includesFocusedAnimal && (contextData[1] == null || contextData[1].Count == 0))
                throw new NotFoundException("Focused animal was not found");

            List<Data.Entities.Animal> datas = contextData[0];
            Data.Entities.Animal focusedAnimal = contextData[1].FirstOrDefault();

            if (includesFocusedAnimal && datas.FirstOrDefault(x => x.Id.Equals(focusedAnimal.Id)) == null) datas.Add(focusedAnimal);
             
            List<Animal> ragContext = await _builderFactory.Builder<AnimalBuilder>().Build(datas, _aiAssistantConfig.ContextFields);

            List<AnimalContext> animalContexts = AnimalContext.FromAnimalModels(ragContext, includesFocusedAnimal ? [request.ContextAnimalId] : null);

            String instructions = await System.IO.File.ReadAllTextAsync(_aiAssistantConfig.InstructionsPath);
            instructions = instructions.Replace(_aiAssistantConfig.RagContextPlaceholder, JsonHelper.SerializeObjectFormattedSafe(animalContexts));

            using MistralClient client = new MistralClient(_aiAssistantConfig.ApiKey);

            List<Mistral.SDK.DTOs.ChatMessage> messages = this.ExtractMessages(request.ConversationHistory, instructions, prompt);

            Mistral.SDK.DTOs.ChatCompletionRequest chatRequest = new Mistral.SDK.DTOs.ChatCompletionRequest()
            {
                Model = ModelDefinitions.MistralMedium,
                Messages = messages,
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

            CompletionsResponse completionsResponse = new CompletionsResponse()
            {
                Response = this.CleanAssistantOutput(response.Choices.FirstOrDefault()?.Message?.Content ?? "No response")
            };

            return completionsResponse;
        }

        private List<Mistral.SDK.DTOs.ChatMessage> ExtractMessages(List<AiMessage> history, String instructions, String prompt)
        {
            List<Mistral.SDK.DTOs.ChatMessage> messages = new List<Mistral.SDK.DTOs.ChatMessage>()
                {
                    new Mistral.SDK.DTOs.ChatMessage()
                    {
                        Role = Mistral.SDK.DTOs.ChatMessage.RoleEnum.System,
                        Content = instructions
                    },
                };

            if (history != null)
            {
                messages.AddRange(history.Select(historyMessage => new Mistral.SDK.DTOs.ChatMessage()
                {
                    Role = historyMessage.Role == Data.Entities.EnumTypes.AiMessageRole.User ? Mistral.SDK.DTOs.ChatMessage.RoleEnum.User : Mistral.SDK.DTOs.ChatMessage.RoleEnum.Assistant,
                    Content = historyMessage.Content
                }));
            }

            messages.Add(new Mistral.SDK.DTOs.ChatMessage()
            {
                Role = Mistral.SDK.DTOs.ChatMessage.RoleEnum.User,
                Content = prompt
            });


            return messages;
        }
        private String CleanAssistantOutput(String raw)
        {
            if (String.IsNullOrWhiteSpace(raw))
                return String.Empty;

            String content = raw;

            // If wrapped in <assistant-output>...</assistant-output>, keep only inner
            Regex assistantWrapperRegex = new Regex(
                @"<assistant-output>(?<inner>[\s\S]*?)</assistant-output>",
                RegexOptions.IgnoreCase
            );
            Match wrapperMatch = assistantWrapperRegex.Match(content);
            if (wrapperMatch.Success)
                content = wrapperMatch.Groups["inner"].Value;

            // Remove fenced code blocks ```...```
            content = Regex.Replace(
                content,
                @"^```[a-zA-Z0-9]*\s*$",
                String.Empty,
                RegexOptions.Multiline
            );

            // Remove markdown headings (#, ##, ...)
            content = Regex.Replace(content, @"^\s*#{1,6}\s.*?$", String.Empty, RegexOptions.Multiline);

            // Remove horizontal rules (---, ___, ***)
            content = Regex.Replace(content, @"^\s*([-_*])\1\1[\1\s]*$", String.Empty, RegexOptions.Multiline);

            // Remove common echoed labels / indicators (existing ones)
            content = Regex.Replace(
                content,
                @"^\s*(Assistant Response\s*\(Text\)|Animal Recommendations\s*\(HTML\)|Suggestions\s*\(Text\)|INPUT\s*\(RAG CONTEXT\)\s*:?)\s*$",
                String.Empty,
                RegexOptions.Multiline | RegexOptions.IgnoreCase
            );

            // Remove echoed headings from the template (strict no-echo policy)
            content = Regex.Replace(
                content,
                @"^\s*(ROLE|MISSION|PLATFORM|DATA\s+CONTEXT|RELEVANCE\s*&\s*FILTERING|EXPLANATION\s+QUALITY|BEHAVIOR\s+RULES|FORMAT\s+RULES|OUTPUT\s+STRUCTURE|INPUT(\s*\(RAG\s*CONTEXT\))?)\s*:?\s*$",
                String.Empty,
                RegexOptions.Multiline | RegexOptions.IgnoreCase
            );

            // Remove stray fences on their own lines
            content = Regex.Replace(content, @"^(```|~~~)\s*$", String.Empty, RegexOptions.Multiline);

            // Safety: strip any nested/duplicated <assistant-output> wrappers that might remain
            content = assistantWrapperRegex.Replace(content, m => m.Groups["inner"].Value);

            // Final tidy
            return content.Trim();
        }




        private String CleanPrompt(String prompt) => Regex.Replace((prompt ?? "").Trim(), @"\s+", " ");
    }
}
