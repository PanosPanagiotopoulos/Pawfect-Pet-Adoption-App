using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.Convention;

namespace Pawfect_Pet_Adoption_App_API.Services.NotificationServices
{
	public class NotificationService : INotificationService
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly INotificationRepository _notificationRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;

		public NotificationService
		(
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
            INotificationRepository notificationRepository,
			IMapper mapper,
			IConventionService conventionService
		)
		{
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _notificationRepository = notificationRepository;
			_mapper = mapper;
			_conventionService = conventionService;
		}

		public async Task<NotificationDto> Persist(NotificationPersist persist, List<String> fields)
		{
			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
			Notification data = new Notification();

			//*TODO* Add authorization service with user roles and permissions

			_mapper.Map(persist, data);
			data.Id = null;
			data.CreatedAt = DateTime.UtcNow;

			String dataId = await _notificationRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατα το Persisting");
			}

			// Return dto model
			NotificationLookup lookup = new NotificationLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

            return (await _builderFactory.Builder<NotificationBuilder>().BuildDto(await lookup.EnrichLookup(_queryFactory).CollectAsync(), fields)).FirstOrDefault();
        }

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			// TODO : Authorization
			await _notificationRepository.DeleteAsync(ids);
		}
	}
}