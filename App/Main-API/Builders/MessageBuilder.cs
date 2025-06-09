using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorization;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Query;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
	public class AutoMessageBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : Message
		public AutoMessageBuilder()
		{
            // Mapping για το Entity : Message σε Message για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Data.Entities.Message, Data.Entities.Message>();

            // POST Request Dto Μοντέλα
            CreateMap<Data.Entities.Message, MessagePersist>();
            CreateMap<MessagePersist, Data.Entities.Message>();
		}
	}

	public class MessageBuilder : BaseBuilder<Models.Message.Message, Data.Entities.Message>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        public MessageBuilder(IQueryFactory queryFactory, IBuilderFactory builderFactory)
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        

        public MessageBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<Models.Message.Message>> Build(List<Data.Entities.Message> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = this.ExtractBuildFields(fields);

			List<String> userFields = new List<String>();
			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
			if (foreignEntitiesFields.ContainsKey(nameof(Models.Message.Message.Recipient)))
				userFields.AddRange(foreignEntitiesFields[nameof(Models.Message.Message.Recipient)]);
            if (foreignEntitiesFields.ContainsKey(nameof(Models.Message.Message.Sender)))
                userFields.AddRange(foreignEntitiesFields[nameof(Models.Message.Message.Sender)]);

            Dictionary<String, List<Models.User.User>>? userMap = userFields.Count > 0 ? await this.CollectUsers(entities, userFields) : null;

            Dictionary<String, Models.Conversation.Conversation>? conversationsMap = foreignEntitiesFields.ContainsKey(nameof(Models.Message.Message.Conversation))
                ? (await CollectConversations(entities,
                                    foreignEntitiesFields[nameof(Models.Message.Message.Conversation)]))
                : null;

            List<Models.Message.Message> result = new List<Models.Message.Message>();
			foreach (Data.Entities.Message e in entities)
			{
                Models.Message.Message dto = new Models.Message.Message();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Models.Message.Message.Content))) dto.Content = e.Content;
				if (nativeFields.Contains(nameof(Models.Message.Message.IsRead))) dto.IsRead = e.IsRead;
				if (nativeFields.Contains(nameof(Models.Message.Message.CreatedAt))) dto.CreatedAt = e.CreatedAt;
                
                if (userMap != null && userMap.TryGetValue(e.Id, out List<Models.User.User> senders) && senders != null && senders.Count > 0) dto.Sender = senders[0];
                if (userMap != null && userMap.TryGetValue(e.Id, out List<Models.User.User> recipients) && recipients != null && recipients.Count > 0) dto.Recipient = recipients[1];
                
                if (conversationsMap != null && conversationsMap.ContainsKey(e.Id)) dto.Conversation = conversationsMap[e.Id];

                result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, List<Models.User.User>>?> CollectUsers(List<Data.Entities.Message> messages, List<String> userFields)
		{
            if (messages.Count == 0 || userFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> userIds = [.. messages.SelectMany(x => new[] { x.SenderId, x.RecipientId }).Distinct()];

            UserLookup userLookup = new UserLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            userLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            userLookup.PageSize = 1000;
            userLookup.Ids = userIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<Models.User.User> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).Build(users, userFields);

            if (userDtos == null || userDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<String, Models.User.User> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα messages δημιουργώντας ένα Dictionary : [ MessageId -> UserId ] 
			return messages.ToDictionary(x => x.Id, x => new List<Models.User.User>() { userDtoMap[x.SenderId], userDtoMap[x.RecipientId] });
		}

        private async Task<Dictionary<String, Models.Conversation.Conversation>> CollectConversations(List<Data.Entities.Message> messages, List<String> conversationFields)
        {
            if (messages.Count == 0 || conversationFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> conversationIds = [.. messages.Select(x => x.ConversationId).Distinct()];

            ConversationLookup conversationLookup = new ConversationLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            conversationLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            conversationLookup.PageSize = 1000;
            conversationLookup.Ids = conversationIds;
            conversationLookup.Fields = conversationFields;

            List<Data.Entities.Conversation> conversations = await conversationLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<Models.Conversation.Conversation> conversationDtos = await _builderFactory.Builder<ConversationBuilder>().Authorise(this._authorise).Build(conversations, conversationFields);

            if (conversationDtos == null || conversationDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<String, Models.Conversation.Conversation> conversationDtoMap = conversationDtos.ToDictionary(x => x.Id);

            // Ταίριασμα του προηγούμενου Dictionary με τα shelters δημιουργώντας ένα Dictionary : [ ShelterId -> UserId ] 
            return messages.ToDictionary(x => x.Id, x => conversationDtoMap[x.ConversationId]);
        }
    }
}