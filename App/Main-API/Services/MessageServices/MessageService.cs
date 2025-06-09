using Amazon.Runtime.Internal.Transform;
using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorization;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using System.Security.Claims;

namespace Pawfect_Pet_Adoption_App_API.Services.MessageServices
{
	public class MessageService : IMessageService
	{
		private readonly IMessageRepository _messageRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public MessageService
		(
			IMessageRepository messageRepository,
			IMapper mapper,
			IConventionService conventionService,
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
			IAuthorizationService AuthorizationService,
			IAuthorizationContentResolver AuthorizationContentResolver,
			ClaimsExtractor claimsExtractor,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder
        )
		{
			_messageRepository = messageRepository;
			_mapper = mapper;
			_conventionService = conventionService;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _authorizationService = AuthorizationService;
            _authorizationContentResolver = AuthorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
        }

		public async Task<Models.Message.Message> Persist(MessagePersist persist, List<String> fields)
		{
            if (!await this.AuthoriseMessagePersist(persist, Permission.EditMessages))
                throw new ForbiddenException("You do not have permission to create messages.", typeof(Data.Entities.Message), Permission.EditMessages);

            Boolean isUpdate = _conventionService.IsValidId(persist.Id);
            Data.Entities.Message data = new Data.Entities.Message();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _messageRepository.FindAsync(x => x.Id == persist.Id);
				if (data == null) throw new NotFoundException("No message found with this id", persist.Id, typeof(Data.Entities.Message));

                _mapper.Map(persist, data);
			}
			else
			{
                _mapper.Map(persist, data);
				data.Id = null; // Ensure new ID is generated
				data.CreatedAt = DateTime.UtcNow;
			}

			if (isUpdate) dataId = await _messageRepository.UpdateAsync(data);
			else dataId = await _messageRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
				throw new InvalidOperationException("Failed to persist message");

			// Return dto model
			MessageLookup lookup = new MessageLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<MessageCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying notifications");

            lookup.Fields = censoredFields;
            return (await _builderFactory.Builder<MessageBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
										 .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
										 .FirstOrDefault();
        }

		private async Task<Boolean> AuthoriseMessagePersist(MessagePersist messagePersist, String permission)
		{
			ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
			String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new Exception("No authenticated user found");

			if (messagePersist.SenderId != userId) return false;

            ConversationLookup lookup = new ConversationLookup();
            lookup.UserIds = new List<String> { messagePersist.RecipientId };

            AffiliatedResource resource = _authorizationContentResolver.BuildAffiliatedResource(lookup, permission);
			return await _authorizationService.AuthorizeOrAffiliatedAsync(resource, permission);
        }

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
            MessageLookup lookup = new MessageLookup();
            lookup.Ids = ids;
            lookup.Offset = 1;
            lookup.PageSize = 10000;
            lookup.Fields = new List<String> {
                                                nameof(Models.Message.Message.Id),
                                                nameof(Models.Message.Message.Sender) + "." + nameof(Models.User.User.Id),
                                             };

            List<Data.Entities.Message> datas = await lookup.EnrichLookup(_queryFactory).CollectAsync();

			OwnedResource ownedResource = _authorizationContentResolver.BuildOwnedResource(new MessageLookup(), [..datas.Select(x => x.SenderId)]);
            if (!await _authorizationService.AuthorizeOrOwnedAsync(ownedResource, Permission.DeleteMessages))
                throw new ForbiddenException("You do not have permission to delete messages.", typeof(Data.Entities.Message), Permission.DeleteMessages);


            await _messageRepository.DeleteAsync(ids);
		}
	}
}