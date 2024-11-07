﻿using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Implementations
{
    public class AnimalRepository : GeneralRepo<Animal>, IAnimalRepository
    {
        public AnimalRepository(MongoDbService dbService) : base(dbService) { }
    }
}
