﻿using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Implementations
{
    public class ShelterRepository : GeneralRepo<Shelter>, IShelterRepository
    {
        public ShelterRepository(MongoDbService dbService) : base(dbService) { }
    }
}
