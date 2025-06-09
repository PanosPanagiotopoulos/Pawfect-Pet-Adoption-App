using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Censors;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorization;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.Convention;

namespace Pawfect_Pet_Adoption_App_API.Services.NotificationServices
{
	public class NotificationService : INotificationService
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly INotificationRepository _notificationRepository;
		private readonly IMapper _mapper;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IConventionService _conventionService;
        private readonly IAuthorizationService _authorizationService;

        public NotificationService
		(
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
            INotificationRepository notificationRepository,
			IMapper mapper,
			ICensorFactory censorFactory,
			AuthContextBuilder contextBuilder,
            IConventionService conventionService,
			IAuthorizationService AuthorizationService
		)
		{
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _notificationRepository = notificationRepository;
			_mapper = mapper;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _conventionService = conventionService;
            _authorizationService = AuthorizationService;
        }

		public async Task<Models.Notification.Notification> Persist(NotificationPersist persist, List<String> fields)
		{
			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
            Data.Entities.Notification data = new Data.Entities.Notification();

			if (!await _authorizationService.AuthorizeAsync(Permission.CreateNotifications))
                throw new ForbiddenException("Unauthorised access when creating notifications", typeof(Data.Entities.Notification), Permission.CreateNotifications);

            _mapper.Map(persist, data);
			data.Id = null;
			data.CreatedAt = DateTime.UtcNow;

			String dataId = await _notificationRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
				throw new InvalidOperationException("Failed to persist notification");

			// Return dto model
			NotificationLookup lookup = new NotificationLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;
            
			AuthContext context = _contextBuilder.OwnedFrom(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<NotificationCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying notifications");

            lookup.Fields = censoredFields;
            return (await _builderFactory.Builder<NotificationBuilder>().Authorise(AuthorizationFlags.OwnerOrPermission)
					.Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermission).CollectAsync(), fields))
					.FirstOrDefault();
        }

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			if (!await _authorizationService.AuthorizeAsync(Permission.DeleteNotifications))
                throw new ForbiddenException("Unauthorised access when deleting notifications", typeof(Data.Entities.Notification), Permission.DeleteNotifications);

            await _notificationRepository.DeleteAsync(ids);
		}
	}
}