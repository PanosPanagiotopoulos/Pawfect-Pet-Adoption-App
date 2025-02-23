﻿using AutoMapper;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Cache;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.EmailServices;
using Pawfect_Pet_Adoption_App_API.Services.HttpServices;
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

		public UserService
		(
			IUserRepository userRepository, IMapper mapper,
			ILogger<UserService> logger, IMemoryCache memoryCache,
			ISmsService smsService, IEmailService emailService,
			RequestService requestService,
			IOptions<CacheConfig> configuration,
			UserQuery userQuery,
			UserBuilder userBuilder
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
		}

		public async Task<String?> RegisterUserUnverifiedAsync(RegisterPersist registerPersist)
		{
			// Ελέγξτε αν ο χρήστης υπάρχει ήδη. Αν ναι, επιστρέψτε το υπάρχον ID του χρήστη.
			if (
				await _userRepository.FindAsync(user => user.Email == registerPersist.User.Email)
				is User existingUser && existingUser != null
				)
			{
				return existingUser.Id;
			}

			// Χαρτογραφήστε τα δεδομένα του χρήστη και ορίστε τον χρήστη ως μη επιβεβαιωμένο.
			User user = _mapper.Map<User>(registerPersist.User);
			user.IsVerified = false;
			user.HasPhoneVerified = false;
			user.HasEmailVerified = false;


			// Αποθηκεύστε τα δεδομένα του χρήστη.
			return await PersistUserAsync(user);
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

			// Αφαιρέστε το υπάρχον token αν υπάρχει.
			if (_memoryCache.TryGetValue(email, out _))
			{
				_memoryCache.Remove(email);
			}

			// Αποθηκεύστε το νέο token στην cache.
			_memoryCache.Set(email, token, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));

			// Δημιουργήστε το URL επιβεβαίωσης.
			String verificationUrl = Path.Join(_requestService.GetBaseURI(), $"auth/verify-email?&token={token}");

			// Στείλτε το email επιβεβαίωσης.
			await _emailService.SendEmailAsync(email, EmailType.Verification.ToString(), String.Format(IEmailService.EmailTemplates[EmailType.Verification], verificationUrl));
		}

		public Boolean VerifyEmail(String? email, String? token)
		{
			if (String.IsNullOrEmpty(email) || String.IsNullOrEmpty(token))
			{
				throw new InvalidDataException("Δεν βρέθηκε email ή token για την επαλήθευση του email");
			}

			// Ελέγξτε αν το token επιβεβαίωσης email υπάρχει στην cache.
			if (!_memoryCache.TryGetValue(email, out String? refreshToken))
			{
				throw new InvalidDataException("Δεν έχει αποσταλθεί email επιβεβαίωσης σε αυτό το email.");
			}

			// Επαληθεύστε το token email.
			if (!(token == refreshToken))
			{
				throw new InvalidDataException("Αυτό το link δεν ισχύει πια");
			}

			// Αφαιρέστε το token από την cache.
			_memoryCache.Remove(email);

			return true;
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

				await PersistUserAsync(user, false);

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

		public async Task<Boolean> ResetPasswordAsync(String? email, String? password, String? token)
		{
			// Επαλήθευση email parameter
			if (String.IsNullOrEmpty(email))
			{
				throw new InvalidDataException("Email είναι απαραίτητο για την επαναφορά του κωδικού.");
			}

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
				// Επαλήθευση του email και του token για να επιλεσει την επαναφορά του κωδικού
				if (!VerifyEmail(email, token))
				{
					throw new InvalidDataException("Η επαλήθευση του email για την επαναφορά κωδικού απέτυχε.");
				}

				User? resetPasswordUser = await RetrieveUserAsync(null, email);

				if (resetPasswordUser == null)
				{
					throw new InvalidDataException("Δεν βρέθηκε χρήστης με αυτό το email.");
				}

				resetPasswordUser.Password = password;
				HashLoginCredentials(ref resetPasswordUser);
				// Κάνουμε update τον χρήστη με τον νέο κωδικό
				await PersistUserAsync(resetPasswordUser, false);

				return true;
			}
			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Αποτυχία επαναφοράς κωδικού");
				return false;
			}

		}

		public async Task<String?> PersistUserAsync(UserPersist userPersist, Boolean allowCreation = true)
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
					HashLoginCredentials(ref workingUser);

					workingUser.CreatedAt = DateTime.UtcNow;
					workingUser.UpdatedAt = DateTime.UtcNow;

					return await _userRepository.AddAsync(workingUser);
				}

				// Ενημερώστε τον υπάρχοντα χρήστη.
				_mapper.Map(userPersist, workingUser);
				workingUser.UpdatedAt = DateTime.UtcNow;
				return await _userRepository.UpdateAsync(workingUser);
			}
			catch (Exception e)
			{
				// LOGS //
				_logger.LogError(e, "Σφάλμα στο persisting του χρήστη : " + e.Message);
				return null;
			}
		}

		public async Task<String?> PersistUserAsync(User user, Boolean allowCreation = true)
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
					HashLoginCredentials(ref workingUser);

					workingUser.CreatedAt = DateTime.UtcNow;
					workingUser.UpdatedAt = DateTime.UtcNow;

					return await _userRepository.AddAsync(workingUser);
				}

				// Ενημερώστε τον υπάρχοντα χρήστη.
				_mapper.Map(user, workingUser);
				workingUser.UpdatedAt = DateTime.UtcNow;
				return await _userRepository.UpdateAsync(user);
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