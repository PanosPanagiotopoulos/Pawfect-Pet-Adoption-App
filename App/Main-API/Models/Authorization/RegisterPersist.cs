﻿using Microsoft.AspNetCore.Mvc;

using Main_API.Models.Shelter;
using Main_API.Models.User;

namespace Main_API.Models
{

	// Μοντέλο για την initial εγγραφή χρήστη ενώς χρήστη στο σύστημα στο σύστημα
	public class RegisterPersist
	{
		public UserPersist User { get; set; }

		public ShelterPersist Shelter { get; set; }
	}
}
