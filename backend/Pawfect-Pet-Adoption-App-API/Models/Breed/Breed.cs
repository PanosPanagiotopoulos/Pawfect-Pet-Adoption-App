﻿using Pawfect_Pet_Adoption_App_API.Models.AnimalType;

namespace Pawfect_Pet_Adoption_App_API.Models.Breed
{
	public class Breed
	{
		public String? Id { get; set; }
		public String? Name { get; set; }
		public AnimalType.AnimalType? AnimalType { get; set; }
		public String? Description { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
