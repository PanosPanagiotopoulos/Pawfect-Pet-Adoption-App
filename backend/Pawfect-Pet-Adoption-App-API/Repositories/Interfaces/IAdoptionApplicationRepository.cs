﻿using Pawfect_Pet_Adoption_App_API.Models;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Interfaces
{
    /// <summary>
    /// Repository όπου διατηρούμε τις μη κοινές λειτουργίες
    /// Για το Collection: AdoptionApplication
    /// </summary>
    public interface IAdoptionApplicationRepository : IGeneralRepo<AdoptionApplication>
    {
    }
}