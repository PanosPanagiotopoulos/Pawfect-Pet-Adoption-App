﻿using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.Animal
{
	public class AnimalPersist
	{
		public String Id { get; set; }
		public String Name { get; set; }
		public double Age { get; set; }
		public Gender Gender { get; set; }
		public String Description { get; set; }
		public double Weight { get; set; }
		public String HealthStatus { get; set; }
		public String ShelterId { get; set; }
		public String BreedId { get; set; }
		public String AnimalTypeId { get; set; }

		// *TODO* Set how the saving , parsing and validation of form file data will be done
		public List<IFormFile> AttachedPhotos { get; set; }
		public AdoptionStatus AdoptionStatus { get; set; }
	}
}
