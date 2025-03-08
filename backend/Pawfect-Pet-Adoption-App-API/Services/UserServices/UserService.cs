using AutoMapper;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Cache;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.EmailServices;
using Pawfect_Pet_Adoption_App_API.Services.HttpServices;
using Pawfect_Pet_Adoption_App_API.Services.ShelterServices;
using Pawfect_Pet_Adoption_App_API.Services.SmsServices;

namespace Pawfect_Pet_Adoption_App_API.Services.UserServices
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<UserService> _logger;
		private readonly IMemoryCache _memoryCache;
		private readonly ISmsService _smsService;
		private readonly IEmailService _emailService;
		private readonly RequestService _requestService;
		private readonly CacheConfig _cacheConfig;
		private readonly UserQuery _userQuery;
		private readonly UserBuilder _userBuilder;
		private readonly Lazy<IShelterService> _shelterService;

		public UserService
		(
			IUserRepository userRepository, IMapper mapper,
			ILogger<UserService> logger, IMemoryCache memoryCache,
			ISmsService smsService, IEmailService emailService,
			RequestService requestService,
			IOptions<CacheConfig> configuration,
			UserQuery userQuery,
			UserBuilder userBuilder,
			Lazy<IShelterService> shelterService
		)
		{
			_userRepository = userRepository;
			_mapper = mapper;
			_logger = logger;
			_memoryCache = memoryCache;
			_smsService = smsService;
			_emailService = emailService;
			_requestService = requestService;
			_cacheConfig = configuration.Value;
			_userQuery = userQuery;
			_userBuilder = userBuilder;
			_shelterService = shelterService;
		}

		public async Task<UserDto?> RegisterUserUnverifiedAsync(RegisterPersist registerPersist)
		{
			_logger.LogInformation(JsonHelper.SerializeObjectFormatted(registerPersist));
			// Ελέγξτε αν ο χρήστης υπάρχει ήδη. Αν ναι, επιστρέψτε το υπάρχον ID του χρήστη.
			if (await _userRepository.ExistsAsync(user => user.Email == registerPersist.User.Email))
			{
				throw new InvalidDataException("Αυτο το email χρησιμοποιείται");
			}

			User user = _mapper.Map<User>(registerPersist.User);
			user.IsVerified = false;

			// Αποθηκεύστε τα δεδομένα του χρήστη.
			UserDto newUser = await Persist(user, true, new() { nameof(UserDto.Id) });
			user.Id = newUser.Id;

			if (newUser == null)
			{
				throw new Exception("Αποτυχια perisiting του νεου χρηστη");
			}

			_logger.LogInformation("New User Created : \n" + JsonHelper.SerializeObjectFormatted(newUser));

			// Save shelter data if any 
			if (registerPersist.Shelter != null)
			{
				registerPersist.Shelter.UserId = newUser.Id;
				ShelterDto newShelter = await _shelterService.Value.Persist(registerPersist.Shelter, new() { nameof(ShelterDto.Id) });
				if (newShelter == null)
				{
					throw new Exception("Αποτυχια persisting shelter");
				}

				user.ShelterId = newShelter.Id;
			}

			return await Persist(user, false);
		}

		public async Task GenerateNewOtpAsync(String? phonenumber)
		{
			if (String.IsNullOrEmpty(phonenumber))
			{
				throw new InvalidDataException("Δεν βρέθηκε αριθμός τηλεφώνου για την αποστολή OTP");
			}

			// Αφαιρέστε το υπάρχον OTP αν υπάρχει.
			if (_memoryCache.TryGetValue<int>(phonenumber, out _))
			{
				_memoryCache.Remove(phonenumber);
			}

			// Δημιουργήστε ένα νέο OTP και αποθηκεύστε το στην cache.
			int newOtp = ISmsService.GenerateOtp();

			_memoryCache.Set(phonenumber, newOtp, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));

			// Στείλτε το OTP μέσω SMS.
			await _smsService.SendSmsAsync(phonenumber, String.Format(ISmsService.SmsTemplates[SmsType.OTP], newOtp));
		}

		public Boolean VerifyOtp(String? phonenumber, int? OTP)
		{
			if (String.IsNullOrEmpty(phonenumber))
			{
				throw new InvalidDataException("Δεν βρέθηκε αριθμός τηλεφώνου για την επαλήθευση OTP");
			}

			if (!OTP.HasValue)
			{
				throw new InvalidDataException("Δεν βρέθηκε απεσταλμένος OTP για την επαλήθευση OTP");
			}

			// Ελέγξτε αν το OTP υπάρχει στην cache.
			if (!_memoryCache.TryGetValue(phonenumber, out int cachedOtp))
			{
				throw new InvalidDataException("Δεν έχει αποσταλθεί μήνυμα OTP σε αυτόν τον αριθμό τηλεφώνου.");
			}

			// Επαληθεύστε το OTP.
			if (!(cachedOtp == OTP.Value))
			{
				throw new InvalidDataException("Λάθος κωδικός OTP");
			}


			// Αφαιρέστε τον κωδικό από την cache.
			_memoryCache.Remove(phonenumber);

			return true;
		}

		public async Task SendVerficationEmailAsync(String? email)
		{
			if (String.IsNullOrEmpty(email))
			{
				throw new InvalidDataException("Δεν βρέθηκε email για την αποστολή του email επιβεβαίωσης");
			}

			// Δημιουργήστε ένα νέο token επιβεβαίωσης email.
			String token = IEmailService.GenerateRefreshToken();

			// Αποθηκεύστε το νέο token στην cache.
			_memoryCache.Set(token, email, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));

			// Δημιουργήστε το URL επιβεβαίωσης.
			String verificationUrl = Path.Join(_requestService.GetFrontendBaseURI(), $"auth/verified?token={token}");

			// Στείλτε το email επιβεβαίωσης.
			await _emailService.SendEmailAsync(email, EmailType.Verification.ToString(), String.Format(IEmailService.EmailTemplates[EmailType.Verification], verificationUrl));
		}

		public String VerifyEmail(String? token)
		{
			if (String.IsNullOrEmpty(token))
			{
				throw new InvalidDataException("Δεν βρέθηκε email ή token για την επαλήθευση του email");
			}

			// Ελέγξτε αν το token επιβεβαίωσης email υπάρχει στην cache.
			if (!_memoryCache.TryGetValue(token, out String email))
			{
				throw new InvalidDataException("Αυτό το link δεν ισχύει πια");
			}

			// Αφαιρέστε το token από την cache.
			_memoryCache.Remove(token);

			return email;
		}

		public async Task<Boolean> VerifyUserAsync(String? id, String? email)
		{
			try
			{
				// Ανακτήστε τον χρήστη με βάση το ID ή το email.
				User? user = await RetrieveUserAsync(id, email);

				// Αν ο χρήστης δεν υπάρχει, ρίξτε εξαίρεση.
				if (user == null)
				{
					throw new Exception("Δεν υπάρχει χρήστης για να εποβεβαιωθεί");
				}

				// Αν ο χρήστης είναι ήδη επιβεβαιωμένος, επιστρέψτε true.
				if (user.IsVerified)
				{
					return true;
				}

				// Επαληθεύστε τον χρήστη με βάση την κατάσταση επιβεβαίωσης τηλεφώνου και email.
				user.IsVerified = user.HasPhoneVerified && user.HasEmailVerified;

				// Αν ο χρήστης δεν είναι επιβεβαιωμένος, καταγράψτε το σφάλμα και επιστρέψτε false.
				if (!user.IsVerified)
				{
					// LOGS //
					_logger.LogError("Ο χρήστη δεν επιβεβαιώθηκε λόγο απουσίας validation κριτηρίων\n" +
									 $"Κριτήρια: Αριθμός Τηλεφώνου : {user.HasPhoneVerified}\n" +
									 $"Κριτήρια: Email : {user.HasEmailVerified}");
					return false;
				}

				// Σε περίπτωση που δεν είναι End-User , θα πρέπει να σταλεί ειδοποίηση σε admin για αν επιβεβαιωθεί
				if (!(user.Role == UserRole.User))
				{
					// TODO: Στείλτε ειδοποίηση στον admin για να επιβεβαιώσει τον χρήστη
				}

				await Persist(user, false, new() { nameof(UserDto.Id) });

				return true;
			}
			catch (Exception e)
			{
				// LOGS //
				_logger.LogError($"Αποτυχία επιβαιβαίωσης χρήστη με σφάλμα:\n{JsonHelper.SerializeObjectFormatted(e)}");
				return false;
			}
		}

		public async Task SendResetPasswordEmailAsync(String? email)
		{
			if (String.IsNullOrEmpty(email))
			{
				throw new InvalidDataException("Δεν βρέθηκε email για την αποστολή του reset email");
			}

			// Κατασκευή καινούριο token
			String token = IEmailService.GenerateRefreshToken();

			// Αφαιρούμε προηγούμενη ενέργεια του χρήστη για επαναφορά
			if (_memoryCache.TryGetValue(email, out _))
			{
				_memoryCache.Remove(email);
			}

			// Store the new token in cache
			_memoryCache.Set(email, token, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));

			// Δημιουργία reset password URL
			String resetPasswordUrl = Path.Join(_requestService.GetBaseURI(), $"auth/reset-password?&token={token}");

			// Κατασκευή θέματος URL. Το σπάμε με κενό για καλύτερο projection στον χρήστη
			String subject = String.Join(' ', EmailType.Reset_Password.ToString().Split('_'));
			// Αποστολή reset password email
			await _emailService.SendEmailAsync(email, subject, String.Format(IEmailService.EmailTemplates[EmailType.Reset_Password], resetPasswordUrl));
		}

		public async Task<Boolean> ResetPasswordAsync(String? password, String? token)
		{
			// Επαλήθευση password parameter
			if (String.IsNullOrEmpty(password))
			{
				throw new InvalidDataException("Ο νέος κωδικός είναι απαραίτητος.");
			}

			// Επαλήθευση token parameter
			if (String.IsNullOrEmpty(token))
			{
				throw new InvalidDataException("Το token επαλήθευσης είναι απαραίτητο.");
			}

			try
			{
				String identifiedEmail = VerifyEmail(token);
				// Επαλήθευση του email και του token για να επιλεσει την επαναφορά του κωδικού
				if (String.IsNullOrEmpty(identifiedEmail))
				{
					throw new InvalidDataException("Η επαλήθευση του email για την επαναφορά κωδικού απέτυχε.");
				}

				User? resetPasswordUser = await RetrieveUserAsync(null, identifiedEmail);

				if (resetPasswordUser == null)
				{
					throw new InvalidDataException("Δεν βρέθηκε χρήστης με αυτό το email.");
				}

				resetPasswordUser.Password = password;
				this.HashLoginCredentials(ref resetPasswordUser);
				// Κάνουμε update τον χρήστη με τον νέο κωδικό
				await Persist(resetPasswordUser, false, new() { nameof(UserDto.Id) });

				return true;
			}
			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Αποτυχία επαναφοράς κωδικού");
				return false;
			}

		}

		public async Task<UserDto?> Persist(UserPersist userPersist, Boolean allowCreation = true, List<String> buildFields = null)
		{
			{
				User? workingUser = null;
				try
				{
					// Ανακτήστε τον χρήστη με βάση το ID ή το email.
					workingUser = await RetrieveUserAsync(userPersist.Id, userPersist.Email);

					// Δημιουργήστε έναν νέο χρήστη αν δεν υπάρχει.
					if (workingUser == null)
					{
						if (!allowCreation)
						{
							// LOGS //
							_logger.LogWarning("Έγινε άπότρεψη προσπάθειας κατασκευής χρήστη χωρίς δικαίωμα κατασκευής");
							return null;
						}

						workingUser = _mapper.Map<User>(userPersist);

						// Hash τα credentials συνθηματικών του χρήστη
						this.HashLoginCredentials(ref workingUser);

						workingUser.CreatedAt = DateTime.UtcNow;
						workingUser.UpdatedAt = DateTime.UtcNow;
						_logger.LogInformation("Working User Persisting:\n", JsonHelper.SerializeObjectFormatted(workingUser));

						workingUser.Id = await _userRepository.AddAsync(workingUser);
						if (String.IsNullOrEmpty(workingUser.Id))
						{
							throw new Exception("Αποτυχία peristing νέου χρήστη");
						}
					}

					else
					{
						_mapper.Map(userPersist, workingUser);
						this.HashLoginCredentials(ref workingUser);
						workingUser.UpdatedAt = DateTime.UtcNow;
						_logger.LogInformation("Working User Persisting:\n", JsonHelper.SerializeObjectFormatted(workingUser));
						await _userRepository.UpdateAsync(workingUser);
					}

					// Return dto model
					UserLookup lookup = new UserLookup(_userQuery);
					lookup.Ids = new List<String> { workingUser.Id };
					lookup.Fields = buildFields ?? new List<String> { "*", nameof(Shelter) + ".*" };
					lookup.Offset = 0;
					lookup.PageSize = 1;

					return (await _userBuilder.SetLookup(lookup).BuildDto(new List<User>() { workingUser }, lookup.Fields.ToList())).FirstOrDefault();
				}
				catch (Exception e)
				{
					// Καταγράψτε το σφάλμα.
					_logger.LogError(e, "Σφάλμα στο persisting του χρήστη");
					return null;
				}
			}
		}

		public async Task<UserDto?> Persist(User user, Boolean allowCreation = true, List<String> buildFields = null)
		{
			User? workingUser = null;
			try
			{
				// Ανακτήστε τον χρήστη με βάση το ID ή το email.
				workingUser = await RetrieveUserAsync(user.Id, user.Email);

				// Δημιουργήστε έναν νέο χρήστη αν δεν υπάρχει.
				if (workingUser == null)
				{
					if (!allowCreation)
					{
						// LOGS //
						_logger.LogWarning("Έγινε άπότρεψη προσπάθειας κατασκευής χρήστη χωρίς δικαίωμα κατασκευής");
						return null;
					}

					workingUser = _mapper.Map<User>(user);

					// Hash τα credentials συνθηματικών του χρήστη
					this.HashLoginCredentials(ref workingUser);

					workingUser.CreatedAt = DateTime.UtcNow;
					workingUser.UpdatedAt = DateTime.UtcNow;
					_logger.LogInformation("Working User Persisting for creation:\n" + JsonHelper.SerializeObjectFormatted(workingUser));

					workingUser.Id = await _userRepository.AddAsync(workingUser);
					if (String.IsNullOrEmpty(workingUser.Id))
					{
						throw new Exception("Αποτυχία peristing νέου χρήστη");
					}
				}

				else
				{
					// Ενημερώστε τον υπάρχοντα χρήστη.
					_mapper.Map(user, workingUser);
					this.HashLoginCredentials(ref workingUser);
					_logger.LogInformation("Working User Persisting for update:\n" + JsonHelper.SerializeObjectFormatted(workingUser));

					workingUser.UpdatedAt = DateTime.UtcNow;
					await _userRepository.UpdateAsync(workingUser);
				}

				// Return dto model
				UserLookup lookup = new UserLookup(_userQuery);
				lookup.Ids = new List<String> { workingUser.Id };
				lookup.Fields = buildFields ?? new List<String> { "*", nameof(Shelter) + ".*" };
				lookup.Offset = 0;
				lookup.PageSize = 1;


				return (await _userBuilder.SetLookup(lookup).BuildDto(new List<User>() { workingUser }, lookup.Fields.ToList())).FirstOrDefault();
			}
			catch (Exception e)
			{
				// Καταγράψτε το σφάλμα.
				_logger.LogError(e, "Σφάλμα στο persisting του χρήστη");
				return null;
			}
		}
		public async Task<User?> RetrieveUserAsync(String? id, String? email)
		{
			// Ανακτήστε τον χρήστη με βάση το ID.
			if (!String.IsNullOrEmpty(id))
			{
				return await _userRepository.FindAsync(user => user.Id == id);
			}
			// Ανακτήστε τον χρήστη με βάση το email.
			else if (!String.IsNullOrEmpty(email))
			{
				return await _userRepository.FindAsync(user => user.Email == email);
			}

			return null;
		}

		// Συνάρτηση για hasing των credentials συνθηματικού του χρήστη
		private void HashLoginCredentials(ref User user)
		{
			switch (user.AuthProvider)
			{
				case AuthProvider.Local:
					user.Password = Security.HashValue(user.Password);
					break;
				default:
					user.AuthProviderId = Security.HashValue(user.AuthProviderId);
					break;
			}
		}

		public async Task<IEnumerable<UserDto>> QueryUsersAsync(UserLookup userLookup)
		{
			List<User> queriedUsers = await userLookup.EnrichLookup(_userQuery).CollectAsync();
			return await _userBuilder.SetLookup(userLookup).BuildDto(queriedUsers, userLookup.Fields.ToList());
		}

		public async Task<UserDto?> Get(String id, List<String> fields)
		{
			UserLookup lookup = new UserLookup(_userQuery);
			lookup.Ids = new List<String> { id };
			lookup.Fields = fields;
			lookup.PageSize = 1;
			lookup.Offset = 0;

			List<User> user = await lookup.EnrichLookup().CollectAsync();

			if (user == null)
			{
				throw new InvalidDataException("Δεν βρέθηκε χρήστης με αυτό το ID");
			}

			return (await _userBuilder.SetLookup(lookup).BuildDto(user, fields)).FirstOrDefault();
		}
	}
}