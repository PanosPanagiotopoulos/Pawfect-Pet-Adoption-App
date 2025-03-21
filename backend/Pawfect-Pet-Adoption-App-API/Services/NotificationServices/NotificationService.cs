﻿using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;

namespace Pawfect_Pet_Adoption_App_API.Services.NotificationServices
{
	public class NotificationService : INotificationService
	{
		private readonly NotificationQuery _notificationQuery;
		private readonly NotificationBuilder _notificationBuilder;
		private readonly INotificationRepository _notificationRepository;
		private readonly IMapper _mapper;

		public NotificationService
		(
			NotificationQuery notificationQuery,
			NotificationBuilder notificationBuilder,
			INotificationRepository notificationRepository,
			IMapper mapper
		)
		{
			_notificationQuery = notificationQuery;
			_notificationBuilder = notificationBuilder;
			_notificationRepository = notificationRepository;
			_mapper = mapper;
		}

		public async Task<IEnumerable<NotificationDto>> QueryNotificationsAsync(NotificationLookup notificationLookup)
		{
			//*TODO* Add authorization service with user roles and permissions

			List<Notification> queriedNotifications = await notificationLookup.EnrichLookup(_notificationQuery).CollectAsync();
			return await _notificationBuilder.SetLookup(notificationLookup).BuildDto(queriedNotifications, notificationLookup.Fields.ToList());
		}

		public async Task<NotificationDto?> Get(String id, List<String> fields)
		{
			NotificationLookup lookup = new NotificationLookup(_notificationQuery);
			lookup.Ids = new List<String> { id };
			lookup.Fields = fields;
			lookup.PageSize = 1;
			lookup.Offset = 0;

			List<Notification> notification = await lookup.EnrichLookup().CollectAsync();

			if (notification == null)
			{
				throw new InvalidDataException("Δεν βρέθηκε ειδοποίηση με αυτό το ID");
			}

			return (await _notificationBuilder.SetLookup(lookup).BuildDto(notification, fields)).FirstOrDefault();
		}

		public async Task<NotificationDto?> Persist(NotificationPersist persist)
		{
			Boolean isUpdate = await _notificationRepository.ExistsAsync(x => x.Id == persist.Id);
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
			NotificationLookup lookup = new NotificationLookup(_notificationQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = new List<String> { "*", nameof(User) + ".*" };
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _notificationBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}
	}
}