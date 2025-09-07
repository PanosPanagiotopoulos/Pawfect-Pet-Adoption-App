using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Query;
using Pawfect_Messenger.Services.FileServices;

namespace Pawfect_Messenger.Builders
{
    public class ConversationBuilder : BaseBuilder<Models.Conversation.Conversation, Data.Entities.Conversation>
    {
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;

        public ConversationBuilder
        (
            IQueryFactory queryFactory,
            IBuilderFactory builderFactory
        )
        {
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;

        public ConversationBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<Models.Conversation.Conversation>> Build(List<Data.Entities.Conversation> entities, List<String> fields)
        {
            // Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
            (List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = this.ExtractBuildFields(fields);

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<String, List<Models.User.User>>? participantsMap = foreignEntitiesFields.ContainsKey(nameof(Models.Conversation.Conversation.Participants))
                ? (await CollectParticipants(entities, foreignEntitiesFields[nameof(Models.Conversation.Conversation.Participants)]))
                : null;

            Dictionary<String, Models.Message.Message?>? lastMessageMap = foreignEntitiesFields.ContainsKey(nameof(Models.Conversation.Conversation.LastMessagePreview))
                ? (await CollectLastMessages(entities, foreignEntitiesFields[nameof(Models.Conversation.Conversation.LastMessagePreview)]))
                : null;

            Dictionary<String, Models.User.User?>? createdByMap = foreignEntitiesFields.ContainsKey(nameof(Models.Conversation.Conversation.CreatedBy))
                ? (await CollectCreatedByUsers(entities, foreignEntitiesFields[nameof(Models.Conversation.Conversation.CreatedBy)]))
                : null;

            List<Models.Conversation.Conversation> result = new List<Models.Conversation.Conversation>();
            foreach (Data.Entities.Conversation e in entities)
            {
                Models.Conversation.Conversation dto = new Models.Conversation.Conversation();
                dto.Id = e.Id;
                if (nativeFields.Contains(nameof(Models.Conversation.Conversation.Type))) dto.Type = e.Type;
                if (nativeFields.Contains(nameof(Models.Conversation.Conversation.LastMessageAt))) dto.LastMessageAt = e.LastMessageAt;
                if (nativeFields.Contains(nameof(Models.Conversation.Conversation.CreatedAt))) dto.CreatedAt = e.CreatedAt;
                if (nativeFields.Contains(nameof(Models.Conversation.Conversation.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;

                if (participantsMap != null && participantsMap.ContainsKey(e.Id)) dto.Participants = participantsMap[e.Id];
                if (lastMessageMap != null && lastMessageMap.ContainsKey(e.Id)) dto.LastMessagePreview = lastMessageMap[e.Id];
                if (createdByMap != null && createdByMap.ContainsKey(e.Id)) dto.CreatedBy = createdByMap[e.Id];

                result.Add(dto);
            }

            return await Task.FromResult(result);
        }

        private async Task<Dictionary<String, List<Models.User.User>>> CollectParticipants(List<Data.Entities.Conversation> conversations, List<String> userFields)
        {
            if (conversations.Count == 0 || userFields.Count == 0) return null;

            // Συλλογή όλων των participant IDs από όλες τις συνομιλίες
            List<String> allParticipantIds = conversations
                .Where(x => x.Participants != null && x.Participants.Count > 0)
                .SelectMany(x => x.Participants)
                .Where(x => !String.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            if (allParticipantIds.Count == 0) return null;

            UserLookup userLookup = new UserLookup();

            userLookup.Offset = 0;
            userLookup.PageSize = 1000;
            userLookup.Ids = allParticipantIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<Models.User.User> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).Build(users, userFields);

            if (userDtos == null || userDtos.Count == 0) return null;

            Dictionary<String, Models.User.User> userDtoMap = userDtos.ToDictionary(x => x.Id);

            // Δημιουργία dictionary που αντιστοιχεί κάθε conversation με τους participants της
            return conversations.ToDictionary(
                x => x.Id,
                x => x.Participants?.Where(id => !String.IsNullOrEmpty(id) && userDtoMap.ContainsKey(id))
                                  .Select(id => userDtoMap[id])
                                  .ToList() ?? new List<Models.User.User>()
            );
        }

        private async Task<Dictionary<String, Models.Message.Message?>?> CollectLastMessages(List<Data.Entities.Conversation> conversations, List<String> messageFields)
        {
            if (conversations.Count == 0 || messageFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των τελευταίων μηνυμάτων
            List<String?> lastMessageIds = [.. conversations
                .Where(x => !String.IsNullOrEmpty(x.LastMessageId))
                .Select(x => x.LastMessageId)
                .Distinct()];

            if (lastMessageIds.Count == 0) return null;

            MessageLookup messageLookup = new MessageLookup();

            messageLookup.Offset = 0;
            messageLookup.PageSize = 1000;
            messageLookup.Ids = lastMessageIds;
            messageLookup.Fields = messageFields;

            List<Data.Entities.Message> messages = await messageLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<Models.Message.Message> messageDtos = await _builderFactory.Builder<MessageBuilder>().Authorise(this._authorise).Build(messages, messageFields);

            if (messageDtos == null || messageDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ MessageId -> MessageDto ]
            Dictionary<String, Models.Message.Message> messageDtoMap = messageDtos.ToDictionary(x => x.Id);

            // Ταίριασμα του προηγούμενου Dictionary με τις conversations δημιουργώντας ένα Dictionary : [ ConversationId -> MessageDto ] 
            return conversations.ToDictionary(x => x.Id, x => !String.IsNullOrEmpty(x.LastMessageId) ? messageDtoMap.GetValueOrDefault(x.LastMessageId) : null);
        }

        private async Task<Dictionary<String, Models.User.User?>?> CollectCreatedByUsers(List<Data.Entities.Conversation> conversations, List<String> userFields)
        {
            if (conversations.Count == 0 || userFields.Count == 0) return null;

            List<String> createdByIds = [.. conversations
                .Where(x => !String.IsNullOrEmpty(x.CreatedBy))
                .Select(x => x.CreatedBy)
                .Distinct()];

            if (createdByIds.Count == 0) return null;

            UserLookup userLookup = new UserLookup();

            userLookup.Offset = 0;
            userLookup.PageSize = 1000;
            userLookup.Ids = createdByIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<Models.User.User> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).Build(users, userFields);

            if (userDtos == null || userDtos.Count == 0) return null;

            Dictionary<String, Models.User.User> userDtoMap = userDtos.ToDictionary(x => x.Id);

            return conversations.ToDictionary(x => x.Id, x => userDtoMap.GetValueOrDefault(x.CreatedBy ?? ""));
        }
    }
}