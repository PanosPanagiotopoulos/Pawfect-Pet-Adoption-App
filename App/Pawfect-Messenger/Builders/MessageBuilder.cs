using AutoMapper;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Query;
using Pawfect_Messenger.Services.AuthenticationServices;
using Pawfect_Messenger.Services.FileServices;

namespace Pawfect_Messenger.Builders
{
    public class MessageBuilder : BaseBuilder<Models.Message.Message, Data.Entities.Message>
    {
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;

        public MessageBuilder
        (
            IQueryFactory queryFactory,
            IBuilderFactory builderFactory
        )
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

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<String, Models.Conversation.Conversation?>? conversationMap = foreignEntitiesFields.ContainsKey(nameof(Models.Message.Message.Conversation))
                ? (await CollectConversations(entities, foreignEntitiesFields[nameof(Models.Message.Message.Conversation)]))
                : null;

            Dictionary<String, Models.User.User?>? senderMap = foreignEntitiesFields.ContainsKey(nameof(Models.Message.Message.Sender))
                ? (await CollectSenders(entities, foreignEntitiesFields[nameof(Models.Message.Message.Sender)]))
                : null;

            Dictionary<String, List<Models.User.User>>? readByMap = foreignEntitiesFields.ContainsKey(nameof(Models.Message.Message.ReadBy))
                ? (await CollectReadByUsers(entities, foreignEntitiesFields[nameof(Models.Message.Message.ReadBy)]))
                : null;

            List<Models.Message.Message> result = new List<Models.Message.Message>();
            foreach (Data.Entities.Message e in entities)
            {
                Models.Message.Message dto = new Models.Message.Message();
                dto.Id = e.Id;
                if (nativeFields.Contains(nameof(Models.Message.Message.Content))) dto.Content = e.Content;
                if (nativeFields.Contains(nameof(Models.Message.Message.Type))) dto.Type = e.Type;
                if (nativeFields.Contains(nameof(Models.Message.Message.Status))) dto.Status = e.Status;
                if (nativeFields.Contains(nameof(Models.Message.Message.CreatedAt))) dto.CreatedAt = e.CreatedAt;
                if (nativeFields.Contains(nameof(Models.Message.Message.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;

                if (conversationMap != null && conversationMap.ContainsKey(e.Id)) dto.Conversation = conversationMap[e.Id];
                if (senderMap != null && senderMap.ContainsKey(e.Id)) dto.Sender = senderMap[e.Id];
                if (readByMap != null && readByMap.ContainsKey(e.Id)) dto.ReadBy = readByMap[e.Id];

                result.Add(dto);
            }

            return await Task.FromResult(result);
        }

        private async Task<Dictionary<String, Models.Conversation.Conversation?>?> CollectConversations(List<Data.Entities.Message> messages, List<String> conversationFields)
        {
            if (messages.Count == 0 || conversationFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String?> conversationIds = [.. messages.Where(x => !String.IsNullOrEmpty(x.ConversationId)).Select(x => x.ConversationId).Distinct()];
            if (conversationIds.Count == 0) return null;

            ConversationLookup conversationLookup = new ConversationLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            conversationLookup.Offset = 0;
            // Γενική τιμή για τη λήψη των dtos
            conversationLookup.PageSize = 1000;
            conversationLookup.Ids = conversationIds;
            conversationLookup.Fields = conversationFields;

            List<Data.Entities.Conversation> conversations = await conversationLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<Models.Conversation.Conversation> conversationDtos = await _builderFactory.Builder<ConversationBuilder>().Authorise(this._authorise).Build(conversations, conversationFields);

            if (conversationDtos == null || conversationDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ ConversationId -> ConversationDto ]
            Dictionary<String, Models.Conversation.Conversation> conversationDtoMap = conversationDtos.ToDictionary(x => x.Id);

            // Ταίριασμα του προηγούμενου Dictionary με τα messages δημιουργώντας ένα Dictionary : [ MessageId -> ConversationDto ] 
            return messages.ToDictionary(x => x.Id, x => !String.IsNullOrEmpty(x.ConversationId) ? conversationDtoMap[x.ConversationId] : null);
        }

        private async Task<Dictionary<String, Models.User.User?>?> CollectSenders(List<Data.Entities.Message> messages, List<String> userFields)
        {
            if (messages.Count == 0 || userFields.Count == 0) return null;

            List<String> senderIds = [.. messages
                .Where(x => !String.IsNullOrEmpty(x.SenderId))
                .Select(x => x.SenderId)
                .Distinct()];

            if (senderIds.Count == 0) return null;

            UserLookup userLookup = new UserLookup();

            userLookup.Offset = 0;
            userLookup.PageSize = 1000;
            userLookup.Ids = senderIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<Models.User.User> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).Build(users, userFields);

            if (userDtos == null || userDtos.Count == 0) return null;

            Dictionary<String, Models.User.User> userDtoMap = userDtos.ToDictionary(x => x.Id);

            return messages.ToDictionary(x => x.Id, x => userDtoMap.GetValueOrDefault(x.SenderId ?? ""));
        }

        private async Task<Dictionary<String, List<Models.User.User>>> CollectReadByUsers(List<Data.Entities.Message> messages, List<String> userFields)
        {
            if (messages.Count == 0 || userFields.Count == 0) return null;

            // Συλλογή όλων των user IDs από τα ReadBy lists
            List<String> allReadByIds = messages
                .Where(x => x.ReadBy != null && x.ReadBy.Count > 0)
                .SelectMany(x => x.ReadBy)
                .Where(x => !String.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            if (allReadByIds.Count == 0) return null;

            UserLookup userLookup = new UserLookup();

            userLookup.Offset = 0;
            userLookup.PageSize = 1000;
            userLookup.Ids = allReadByIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<Models.User.User> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).Build(users, userFields);

            if (userDtos == null || userDtos.Count == 0) return null;

            Dictionary<String, Models.User.User> userDtoMap = userDtos.ToDictionary(x => x.Id);

            // Δημιουργία dictionary που αντιστοιχεί κάθε message με τους users που το έχουν διαβάσει
            return messages.ToDictionary(
                x => x.Id,
                x => x.ReadBy?.Where(id => !String.IsNullOrEmpty(id) && userDtoMap.ContainsKey(id))
                           .Select(id => userDtoMap[id])
                           .ToList() ?? new List<Models.User.User>()
            );
        }
    }
}