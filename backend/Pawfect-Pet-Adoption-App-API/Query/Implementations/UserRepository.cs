﻿using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;
namespace Pawfect_Pet_Adoption_App_API.Repositories.Implementations
{
	public class UserRepository : GeneralRepo<User>, IUserRepository
	{
		public UserRepository(MongoDbService dbService, IHttpContextAccessor httpContextAccessor) : base(dbService, httpContextAccessor) { }

		public async Task<IEnumerable<User>> GetAllAsync()
		{
			return await _collection.Find(_ => true).ToListAsync();
		}

	}
}
