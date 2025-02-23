using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;

namespace Pawfect_Pet_Adoption_App_API.Services.ConversationServices
{
	public class ConversationService : IConversationService
	{
		private readonly ConversationQuery _conversationQuery;
		private readonly ConversationBuilder _conversationBuilder;
		private readonly IConversationRepository _conversationRepository;
		private readonly IMapper _mapper;


		public ConversationService
		(
			ConversationQuery conversationQuery,
			ConversationBuilder conversationBuilder,
			IConversationRepository conversationRepository,
			IMapper mapper
		)
		{
			_conversationQuery = conversationQuery;
			_conversationBuilder = conversationBuilder;
			_conversationRepository = conversationRepository;
			_mapper = mapper;
		}

		public async Task<IEnumerable<ConversationDto>> QueryConversationsAsync(ConversationLookup conversationLookup)
		{
			List<Conversation> queriedConversations = await conversationLookup.EnrichLookup(_conversationQuery).CollectAsync();
			return await _conversationBuilder.SetLookup(conversationLookup).BuildDto(queriedConversations, conversationLookup.Fields.ToList());
		}

		public async Task<ConversationDto?> Persist(ConversationPersist persist)
		{
			Boolean isUpdate = await _conversationRepository.ExistsAsync(x => x.Id == persist.Id);
			Conversation data = new Conversation();
			String dataId = String.Empty;
			if (isUpdate)
			{
				_mapper.Map(persist, data);
				dataId = await _conversationRepository.UpdateAsync(data);
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null; // Ensure new ID is generated
				data.CreatedAt = DateTime.UtcNow;
				dataId = await _conversationRepository.AddAsync(data);
			}

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατά το persist της συζήτησης");
			}

			// Return dto model
			ConversationLookup lookup = new ConversationLookup(_conversationQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = new List<String> { "*", nameof(User) + ".*", nameof(Animal) + ".*" };
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _conversationBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}
	}
}