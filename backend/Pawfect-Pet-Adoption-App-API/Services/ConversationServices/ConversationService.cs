using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Services.ConversationServices
{
	public class ConversationService : IConversationService
	{
		private readonly ConversationQuery _conversationQuery;
		private readonly ConversationBuilder _conversationBuilder;

		public ConversationService(ConversationQuery conversationQuery, ConversationBuilder conversationBuilder)
		{
			_conversationQuery = conversationQuery;
			_conversationBuilder = conversationBuilder;
		}

		public async Task<IEnumerable<ConversationDto>> QueryConversationsAsync(ConversationLookup conversationLookup)
		{
			List<Conversation> queriedConversations = await conversationLookup.EnrichLookup(_conversationQuery).CollectAsync();
			return await _conversationBuilder.SetLookup(conversationLookup).BuildDto(queriedConversations, conversationLookup.Fields.ToList());
		}
	}
}