using AutoMapper;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using MongoDB.Driver;
using Newtonsoft.Json;
using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.Apis;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Data.Entities.Types.Cache;
using Pawfect_API.DevTools;
using Pawfect_API.Exceptions;
using Pawfect_API.Models;
using Pawfect_API.Models.File;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Models.User;
using Pawfect_API.Query;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.FileServices;
using Pawfect_API.Services.HttpServices;
using Pawfect_API.Services.ShelterServices;
using System.Security.Claims;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_API.Services.Convention;
using Pawfect_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis;
using Pawfect_API.Services.NotificationServices;
using Pawfect_Pet_Adoption_App_API.DevTools;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using MongoDB.Driver.Core.Servers;
using Pawfect_Pet_Adoption_App_API.Models.Authorization;

namespace Pawfect_API.Services.UserServices
{
	public class UserService : IUserService
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<UserService> _logger;
		private readonly IMemoryCache _memoryCache;
		private readonly RequestService _requestService;
		private readonly CacheConfig _cacheConfig;
		private readonly Lazy<IShelterService> _shelterService;
		private readonly IAuthenticationService _authenticationService;
		private readonly Lazy<IFileService> _fileService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IShelterRepository _shelterRepository;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly IConventionService _conventionService;
        private readonly INotificationApiClient _notificationApiClient;
        private readonly NotificationApiConfig _notificationConfig;

        public UserService
		(
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
            IUserRepository userRepository, IMapper mapper,
			ILogger<UserService> logger, IMemoryCache memoryCache,
			RequestService requestService,
			IOptions<CacheConfig> configuration,
			Lazy<IShelterService> shelterService,
			IAuthenticationService authenticationService,
			Lazy<IFileService> fileService,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
			IShelterRepository shelterRepository,
			ClaimsExtractor claimsExtractor,
			IAuthorizationService AuthorizationService,
			IAuthorizationContentResolver authorizationContentResolver,
            IConventionService conventionService,
			IOptions<NotificationApiConfig> notificationOptions,
			INotificationApiClient notificationApiClient
        )
		{
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _userRepository = userRepository;
			_mapper = mapper;
			_logger = logger;
			_memoryCache = memoryCache;
			_requestService = requestService;
			_cacheConfig = configuration.Value;
			_shelterService = shelterService;
			_authenticationService = authenticationService;
			_fileService = fileService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _shelterRepository = shelterRepository;
            _claimsExtractor = claimsExtractor;
            _authorizationService = AuthorizationService;
            _authorizationContentResolver = authorizationContentResolver;
            _conventionService = conventionService;
            _notificationApiClient = notificationApiClient;
            _notificationConfig = notificationOptions.Value;
        }

		public async Task<Models.User.User?> RegisterUserUnverifiedAsync(RegisterPersist registerPersist, List<String> fields)
		{
			// Ελέγξτε αν ο χρήστης υπάρχει ήδη. Αν ναι, επιστρέψτε το υπάρχον ID του χρήστη.
			if (await _userRepository.ExistsAsync(user => user.Email == registerPersist.User.Email))
				throw new ArgumentException("This email is already in use");

			// Αποθηκεύστε τα δεδομένα του χρήστη.
			if (registerPersist.User.AuthProvider != AuthProvider.Local)
				registerPersist.User.HasEmailVerified = true;

            Models.User.User newUser = await this.Persist(registerPersist.User, true, buildDto: true, buildFields: fields);

			if (newUser == null)
				throw new NotFoundException("Failed to persist user", null, typeof(Data.Entities.User));

			// Save shelter data if any 
			if (registerPersist.Shelter != null)
			{
				registerPersist.Shelter.UserId = newUser.Id;
                Models.Shelter.Shelter newShelter = await _shelterService.Value.Persist(registerPersist.Shelter, new() { nameof(Models.Shelter.Shelter.Id) });
				if (newShelter == null)
					throw new NotFoundException("Failed to persist Shelter", null,  typeof(Data.Entities.Shelter));

				return await this.Get(newUser.Id, fields);
			}

			return newUser;
		}

		private async Task PersistProfilePhoto(String profilePhoto, String oldProfilePhoto, String ownerId, Boolean justCreated = false)
		{
			// If not new user who persists profile photo, include authorization
			if (!justCreated)
			{
                ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
                String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
                if (String.IsNullOrEmpty(userId)) throw new UnAuthenticatedException();

				if (!String.Equals(ownerId, userId))
					throw new ForbiddenException("Unable to persist profile photo from another user , only the owner can");
            }

			Boolean emptyOldPhoto = String.IsNullOrEmpty(oldProfilePhoto);
			Boolean emptyNewPhoto = String.IsNullOrEmpty(profilePhoto);

			if (!emptyOldPhoto)
			{
				// If the profile photo was deleted
				if (emptyNewPhoto)
					await _fileService.Value.Delete(oldProfilePhoto);
			}

			// Is empty, means it got deleted, so no need to query for persisting
			if (emptyNewPhoto) return;
			if (profilePhoto.Equals(oldProfilePhoto)) return;

			FileLookup lookup = new FileLookup();
			lookup.Ids = new List<String>() { profilePhoto };
			lookup.Offset = 1;
			lookup.PageSize = 1;
			
			Data.Entities.File profilePhotoFile = (await lookup.EnrichLookup(_queryFactory).CollectAsync()).FirstOrDefault();
			if (profilePhotoFile == null)
			{
				_logger.LogError("Failed to saved attached files. No return from query");
				return;
			}
			
			profilePhotoFile.FileSaveStatus = FileSaveStatus.Permanent;
			profilePhotoFile.OwnerId = ownerId;

			await _fileService.Value.Persist
			(
				_mapper.Map<FilePersist>(profilePhotoFile),
				new List<String>() { nameof(Models.File.File.Id) },
				false
			);
		}

		public async Task SendOtpAsync(String phonenumber , String userId)
		{
			if (String.IsNullOrEmpty(phonenumber))
				throw new ArgumentException("No phone number found");

			// Αφαιρέστε το υπάρχον OTP αν υπάρχει.
			if (_memoryCache.TryGetValue<int>(phonenumber, out _))
				_memoryCache.Remove(phonenumber);

			// Δημιουργήστε ένα νέο OTP και αποθηκεύστε το στην cache.
			int newOtp = new Random().Next(100000, 999999);

            Dictionary<String, String> titleMappings = new Dictionary<String, String>();
            Dictionary<String, String> contentMappings = new Dictionary<String, String>()
            {
                {
                    _notificationConfig.OtpPasswordPlaceholders.OtpPassword,
					newOtp.ToString()
                },
            };

            NotificationEvent sendOtpEvent = new NotificationEvent()
            {
                UserId = userId,
                Type = NotificationType.Sms,
                TitleMappings = titleMappings,
                ContentMappings = contentMappings,
                TeplateId = _notificationConfig.OtpPasswordPlaceholders.TemplateId
            };

            await _notificationApiClient.NotificationEvent(sendOtpEvent);

            _memoryCache.Set(phonenumber, newOtp, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));
		}

		public Boolean VerifyOtp(String phonenumber, int? OTP)
		{
			if (String.IsNullOrEmpty(phonenumber))
				throw new ArgumentException("No phonenumber found");

			if (!OTP.HasValue)
				throw new ArgumentException("No OTP code found");

			// Ελέγξτε αν το OTP υπάρχει στην cache.
			if (!_memoryCache.TryGetValue(phonenumber, out int cachedOtp))
				throw new NotFoundException("No cached otp found for given phonenumber", phonenumber);

			// Επαληθεύστε το OTP.
			if (!(cachedOtp == OTP.Value))
			{
				return false;
			}


			// Αφαιρέστε τον κωδικό από την cache.
			_memoryCache.Remove(phonenumber);

			return true;
		}

		public async Task SendVerficationEmailAsync(String email)
		{
			if (String.IsNullOrEmpty(email)) throw new ArgumentException("No email found to send verification email");

			// Δημιουργήστε ένα νέο token επιβεβαίωσης email.
			String token = Guid.NewGuid().ToString();

			Data.Entities.User user = await _userRepository.FindAsync(user => user.Email == email, [nameof(Data.Entities.User.Id), nameof(Data.Entities.User.FullName)]);

			Dictionary<String, String> titleMappings = new Dictionary<String, String>();
            Dictionary<String, String> contentMappings = new Dictionary<String, String>()
			{
				{
					_notificationConfig.VerificationEmailPlaceholders.FirstName,
                    UserDataHelper.GetFirstNameFormatted(user.FullName)
                },
                {
                    _notificationConfig.VerificationEmailPlaceholders.VerificationToken,
                    token
                }
            };

			NotificationEvent sendEmailEvent = new NotificationEvent()
			{
				UserId = user.Id,
				Type = NotificationType.Email,
				TitleMappings = titleMappings,
				ContentMappings = contentMappings,
				TeplateId = _notificationConfig.VerificationEmailPlaceholders.TemplateId
            };

			await _notificationApiClient.NotificationEvent(sendEmailEvent);	

            // Αποθηκεύστε το νέο token στην cache.
            _memoryCache.Set(token, email, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));
		}

		public String VerifyEmail(String token)
		{
			if (String.IsNullOrEmpty(token))
				throw new ArgumentException("Token not given");

			// Ελέγξτε αν το token επιβεβαίωσης email υπάρχει στην cache.
			if (!_memoryCache.TryGetValue(token, out String email))
			{
				throw new ArgumentException("No email found for given token");
			}

			// Αφαιρέστε το token από την cache.
			_memoryCache.Remove(token);

			return email;
		}

		public async Task<Boolean> VerifyUserAsync(String id, String email)
	{
            // Ανακτήστε τον χρήστη με βάση το ID ή το email.
            Data.Entities.User user = await this.RetrieveUserAsync(id, email);

			// Αν ο χρήστης δεν υπάρχει, ρίξτε εξαίρεση.
			if (user == null)
				throw new NotFoundException("No user found with that credentials to verify", $"{id}_{email}", typeof(Data.Entities.User));

			// Αν ο χρήστης είναι ήδη επιβεβαιωμένος, επιστρέψτε true.
			if (user.IsVerified) return true;	

			// Επαληθεύστε τον χρήστη με βάση την κατάσταση επιβεβαίωσης τηλεφώνου και email.
			user.IsVerified = user.HasPhoneVerified && (user.HasEmailVerified || user.AuthProvider != AuthProvider.Local);

			// Αν ο χρήστης δεν είναι επιβεβαιωμένος, καταγράψτε το σφάλμα και επιστρέψτε false.
			if (!user.IsVerified)
			{
				// LOGS //
				_logger.LogError("User does not match all the needed verification requirements\n" +
									$"PhoneNumber verified : {user.HasPhoneVerified}\n" +
									$"Email Verified : {user.HasEmailVerified}");
				return false;
			}

			// Σε περίπτωση που δεν είναι End-User , θα πρέπει να σταλεί ειδοποίηση σε admin για αν επιβεβαιωθεί
			if (user.Roles.Any(role => role != UserRole.User))
			{
				user.IsVerified = false;
				await this.SendAdminVerifyNotificationAsync(user);

			}

			await Persist(user, false, buildDto: false);

			return true;
		}

		public async Task<Boolean> VerifyShelterAsync(AdminVerifyPayload payload)
		{
			String cacheKey = $"verify_user_email_admin_{payload.AdminToken}";
			if (!_memoryCache.TryGetValue(cacheKey, out String adminUserData))
				throw new ForbiddenException();
			
			// [0]: admin id, [1] user id
			String[] adminUserValues = adminUserData.Split('_');

			Data.Entities.User toVeirfyUser = await this.RetrieveUserAsync(adminUserValues[1], null);
			toVeirfyUser.IsVerified = true;

			Data.Entities.Shelter toVerifyShelter = await _shelterRepository.FindAsync(x => x.UserId.Equals(adminUserValues[1]));
			toVerifyShelter.VerificationStatus = payload.Accept ? VerificationStatus.Verified : VerificationStatus.Rejected;
            toVerifyShelter.VerifiedById = adminUserValues[0];

            String[] results = await Task.WhenAll(_userRepository.UpdateAsync(toVeirfyUser), _shelterRepository.UpdateAsync(toVerifyShelter));
			// Validate update sucess
			if (results == null || results.Length != 2 || results.Any(String.IsNullOrEmpty)) throw new InvalidOperationException("Failed to update shelter verification");

			return true;
		}


        private async Task SendAdminVerifyNotificationAsync(Data.Entities.User user)
		{
			// Fetch shelter data
			if (!user.Roles.Contains(UserRole.Shelter) || String.IsNullOrEmpty(user.ShelterId)) return;

			Data.Entities.Shelter shelter = await _shelterRepository.FindAsync(x => x.Id == user.ShelterId);
			Data.Entities.User admin = await _userRepository.FindAsync(x => x.Roles.Contains(UserRole.Admin), [nameof(Data.Entities.User.Id)]);

			String adminToken = Guid.NewGuid().ToString();

            Dictionary<String, String> titleMappings = new Dictionary<String, String>()
			{
				{
					_notificationConfig.VerifyUserPlaceholders.ShelterName,
					shelter.ShelterName
				},
			};
            Dictionary<String, String> contentMappings = new Dictionary<String, String>()
            {
                {
                    _notificationConfig.VerifyUserPlaceholders.AdminToken,
                    adminToken
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.ShelterName,
                    shelter.ShelterName
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.ShelterId,
                    shelter.Id
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.Description,
                    shelter.Description
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.UserId,
                    user.Id
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.RegistrationDate,
                    user.CreatedAt.ToString()
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.Website,
                    shelter.Website ?? "None"
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.SocialMediaFacebook,
                    shelter.SocialMedia?.Facebook ?? "None"
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.SocialMediaInstagram,
                    shelter.SocialMedia?.Instagram ?? "None"
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.OperatingHoursMonday,
                    shelter.OperatingHours?.Monday ?? "None"
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.OperatingHoursTuesday,
                    shelter.OperatingHours?.Tuesday ?? "None"
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.OperatingHoursWednesday,
                    shelter.OperatingHours?.Wednesday ?? "None"
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.OperatingHoursThursday,
                    shelter.OperatingHours?.Thursday ?? "None"
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.OperatingHoursFriday,
                    shelter.OperatingHours?.Friday ?? "None"
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.OperatingHoursSaturday,
                    shelter.OperatingHours?.Saturday ?? "None"
                },
                {
                    _notificationConfig.VerifyUserPlaceholders.OperatingHoursSunday,
                    shelter.OperatingHours?.Sunday ?? "None"
                }
            };

            NotificationEvent sendEmailEvent = new NotificationEvent()
            {
                UserId = admin.Id,
                Type = NotificationType.Email,
                TitleMappings = titleMappings,
                ContentMappings = contentMappings,
                TeplateId = _notificationConfig.VerifyUserPlaceholders.TemplateId
            };

            await _notificationApiClient.NotificationEvent(sendEmailEvent);

            _memoryCache.Set($"verify_user_email_admin_{adminToken}", $"{admin.Id}_{user.Id}", TimeSpan.FromHours(_cacheConfig.AdminVerificationCacheTime));
        }


        public async Task SendResetPasswordEmailAsync(String email)
		{
			if (String.IsNullOrEmpty(email))
				throw new ArgumentException("No email found to send reset password email");

            // Δημιουργήστε ένα νέο token επιβεβαίωσης email.
            String token = Guid.NewGuid().ToString();

            Data.Entities.User user = await _userRepository.FindAsync(user => user.Email == email, [nameof(Data.Entities.User.Id), nameof(Data.Entities.User.FullName)]);

			String firstName = UserDataHelper.GetFirstNameFormatted(user.FullName);
            Dictionary<String, String> titleMappings = new Dictionary<String, String>()
			{
				{
                    _notificationConfig.ResetPasswordEmailPlaceholders.FirstName,
                    firstName
                }
				
            };
            Dictionary<String, String> contentMappings = new Dictionary<String, String>()
            {
                {
                    _notificationConfig.VerificationEmailPlaceholders.FirstName,
                    firstName
                },
                {
                    _notificationConfig.VerificationEmailPlaceholders.VerificationToken,
                    token
                }
            };

            NotificationEvent sendEmailEvent = new NotificationEvent()
            {
                UserId = user.Id,
                Type = NotificationType.Email,
                TitleMappings = titleMappings,
                ContentMappings = contentMappings,
                TeplateId = _notificationConfig.ResetPasswordEmailPlaceholders.TemplateId
            };

            await _notificationApiClient.NotificationEvent(sendEmailEvent);

            // Store the new token in cache
            _memoryCache.Set(token, email, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));
        }

        public async Task<String> VerifyResetPasswordToken(String token)
		{
			if (String.IsNullOrEmpty(token))
				throw new ArgumentException("No token found to reset password");
			
			// Ελέγξτε αν το token επαλήθευσης reset password υπάρχει στην cache.
			if (!_memoryCache.TryGetValue(token, out String email))
				throw new ArgumentException("This reset password token is not valid anymore");
			
			// Αφαιρέστε το token από την cache.
			_memoryCache.Remove(token);

			return await Task.FromResult(email);
		}

		public async Task<Boolean> ResetPasswordAsync(String email, String password)
		{
			// Επαλήθευση password parameter
			if (String.IsNullOrEmpty(password))
				throw new ArgumentException("A new password is required to reset password");

			// Επαλήθευση token parameter
			if (String.IsNullOrEmpty(email))
				throw new ArgumentException("The email is required for the password to reset");

				Data.Entities.User resetPasswordUser = await this.RetrieveUserAsync(null, email);

				if (resetPasswordUser == null)
					throw new NotFoundException("No user was found to reset password with this email", email, typeof(Data.Entities.User));

				resetPasswordUser.Password = password;
				this.HashLoginCredentials(ref resetPasswordUser);
				// Κάνουμε update τον χρήστη με τον νέο κωδικό
				await Persist(resetPasswordUser, false, buildDto: false);

				return true;
		}
		public async Task<(String, String)> RetrieveGoogleCredentials(String authorizationCode)
		{
            Data.Entities.User userInfo = await this.GetGoogleUser(authorizationCode);
			if (userInfo == null)
				throw new InvalidOperationException("Failed to retrieve google user data");

			return (userInfo.Email, userInfo.AuthProviderId);
		}

		public async Task<Data.Entities.User> GetGoogleUser(String authorizationCode)
		{
			if (String.IsNullOrEmpty(authorizationCode))
				throw new ArgumentException("No access code given to get google user");

			GoogleTokenResponse tokenResponse = await _authenticationService.ExchangeCodeForAccessToken(authorizationCode);
			if (tokenResponse == null)
				throw new ArgumentException("Failed to exchange Google authorization code for access token.");

            Data.Entities.User userInfo = await this.FetchUserInfoFromGoogle(tokenResponse.AccessToken);
			this.HashLoginCredentials(ref userInfo);
			if (userInfo == null)
				throw new NotFoundException("Failed to retrieve user info from Google.", null, typeof(Data.Entities.User));

			return userInfo;
		}

		private async Task<Data.Entities.User> FetchUserInfoFromGoogle(String accessToken)
		{
			using (HttpClient client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", $"{accessToken}");

				// People API endpoint with requested fields
				String endpoint = "https://people.googleapis.com/v1/people/me?personFields=names,emailAddresses,phoneNumbers,addresses,metadata";
				HttpResponseMessage response = await client.GetAsync(endpoint);

				if (!response.IsSuccessStatusCode)
				{
					String errorContent = await response.Content.ReadAsStringAsync();
					throw new InvalidOperationException($"Failed to fetch user info from Google: {errorContent}");
				}

				String responseContent = await response.Content.ReadAsStringAsync();
				dynamic person = JsonConvert.DeserializeObject(responseContent);

				if (person == null)
                    throw new InvalidOperationException("Failed to find deserialized Google user info.");

                String sub = person.metadata?.sources?[0]?.id?.ToString();
				String name = person.names?.Count > 0 ? person.names[0].displayName?.ToString() : null;
				String email = person.emailAddresses?.Count > 0 ? person.emailAddresses[0].value?.ToString() : null;
				String phoneNumber = person.phoneNumbers?.Count > 0 ? person.phoneNumbers[0].value?.ToString() : null;
				String address = person.addresses?.Count > 0 ? person.addresses[0].formattedValue?.ToString() : null;

				return new Data.Entities.User
				{
					Id = null,
					ShelterId = null,
					FullName = name,
					Email = email,
					Password = "",
					Phone = phoneNumber,
					Location = Location.FromGoogle(responseContent),
					AuthProvider = AuthProvider.Google,
					AuthProviderId = sub,
				};
			}
		}

		public async Task<Models.User.User?> Update(UserUpdate model, List<String> buildFields = null, Boolean buildDto = true, Boolean auth = true)
		{
            Data.Entities.User? workingUser = await this.RetrieveUserAsync(model.Id, null);
			if (workingUser == null) throw new NotFoundException();

			if (auth)
			{
                ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
                String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
                if (String.IsNullOrEmpty(userId)) throw new UnAuthenticatedException();

                OwnedResource ownedResource = new OwnedResource(userId, new OwnedFilterParams(new UserLookup()));
                if (!await _authorizationService.AuthorizeOrOwnedAsync(ownedResource, Permission.EditUsers))
                    throw new ForbiddenException();
            }

            String oldProfilePhoto = workingUser?.ProfilePhotoId;

            _mapper.Map(model, workingUser);

            workingUser.UpdatedAt = DateTime.UtcNow;

			if (String.IsNullOrEmpty(await _userRepository.UpdateAsync(workingUser)))
                throw new InvalidOperationException("Failed to update user");

            await this.PersistProfilePhoto(model.ProfilePhotoId, oldProfilePhoto, workingUser.Id, justCreated: !auth);

            Models.User.User persisted = null;
            if (buildDto)
            {
                // Return dto model
                UserLookup lookup = new UserLookup();
                lookup.Ids = new List<String> { workingUser.Id };
                lookup.Fields = buildFields ?? new List<String> { nameof(Models.User.User.FullName) };
                lookup.Offset = 1;
                lookup.PageSize = 1;

				if (auth)
				{
                    AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
                    List<String> censoredFields = await _censorFactory.Censor<UserCensor>().Censor([.. lookup.Fields], context);
                    if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying users");
                    lookup.Fields = censoredFields;
                }

                persisted = (await _builderFactory.Builder<UserBuilder>()
                    .Build(await lookup.EnrichLookup(_queryFactory).CollectAsync(), [.. lookup.Fields]))
                    .FirstOrDefault();
            }

            return persisted;
        }


        public async Task<Models.User.User> Persist(UserPersist userPersist, Boolean allowCreation = true, List<String> buildFields = null, Boolean buildDto = true)
		{
			if (_conventionService.IsValidId(userPersist.Id))
				return await this.Update(
					new UserUpdate()
					{
						Id  = userPersist.Id,
						Email = userPersist.Email,
						FullName = userPersist.FullName,
						Location = userPersist.Location,
						Phone = userPersist.Phone,
						ProfilePhotoId = userPersist.ProfilePhotoId
					},
					buildFields,
					buildDto,
					false
				);


                Data.Entities.User workingUser = _mapper.Map<Data.Entities.User>(userPersist);

				// Get all needed access roles
				if (userPersist.Role == UserRole.User)
                    workingUser.Roles = new List<UserRole> { UserRole.User };
                else if (userPersist.Role == UserRole.Shelter)
                    workingUser.Roles = new List<UserRole> { UserRole.User, UserRole.Shelter };
                else if (userPersist.Role == UserRole.Admin)
                    workingUser.Roles = new List<UserRole> { UserRole.User, UserRole.Shelter, UserRole.Admin };

                // Hash τα credentials συνθηματικών του χρήστη
                this.HashLoginCredentials(ref workingUser);

				workingUser.CreatedAt = DateTime.UtcNow;
				workingUser.UpdatedAt = DateTime.UtcNow;

				workingUser.Id = await _userRepository.AddAsync(workingUser);

            if (String.IsNullOrEmpty(workingUser.Id))
                throw new InvalidOperationException("Persisting failed and user was not found");

            await this.PersistProfilePhoto(userPersist.ProfilePhotoId, null, workingUser.Id, justCreated: true);

            Models.User.User persisted = new Models.User.User();
			if (buildDto)
			{
				// Return dto model
				UserLookup lookup = new UserLookup();
				lookup.Ids = new List<String> { workingUser.Id };
				lookup.Fields = buildFields ?? new List<String> { nameof(Models.User.User.FullName) };
				lookup.Offset = 1;
				lookup.PageSize = 1;

                persisted = (await _builderFactory.Builder<UserBuilder>()
                    .Build(await lookup.EnrichLookup(_queryFactory).CollectAsync(), [.. lookup.Fields]))
                    .FirstOrDefault();
            }

			return persisted;
			
		}
		public async Task<Models.User.User> Persist(Data.Entities.User user, Boolean allowCreation = true, List<String> buildFields = null, Boolean buildDto = true)
		{
            if (_conventionService.IsValidId(user.Id))
                return await this.Update(
                    new UserUpdate()
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        Location = user.Location,
                        Phone = user.Phone,
                        ProfilePhotoId = user.ProfilePhotoId,
						HasPhoneVerified = user.HasPhoneVerified,
						HasEmailVerified = user.HasEmailVerified,
						IsVerified = user.IsVerified
                    },
                    buildFields,
                    buildDto,
					false
                );

            Data.Entities.User workingUser = _mapper.Map<Data.Entities.User>(user);
			// Hash τα credentials συνθηματικών του χρήστη
			this.HashLoginCredentials(ref workingUser);

			workingUser.CreatedAt = DateTime.UtcNow;
			workingUser.UpdatedAt = DateTime.UtcNow;

            if (String.IsNullOrEmpty(workingUser.Id))
                throw new InvalidOperationException("Persisting failed and user was not found");

			workingUser.Id = await _userRepository.AddAsync(workingUser);

            if (String.IsNullOrEmpty(workingUser.Id))
                throw new InvalidOperationException("Persisting failed and user was not found");

            await this.PersistProfilePhoto(user.ProfilePhotoId, workingUser.ProfilePhotoId, workingUser.Id, justCreated: true);

            Models.User.User persisted = new Models.User.User();
			if (buildDto)
			{
				// Return dto model
				UserLookup lookup = new UserLookup();
				lookup.Ids = new List<String> { workingUser.Id };
				lookup.Fields = buildFields ?? new List<String> { "*", nameof(Models.Shelter.Shelter) + ".*" };
				lookup.Offset = 1;
				lookup.PageSize = 1;

                persisted = (await _builderFactory.Builder<UserBuilder>()
					.Build(await lookup.EnrichLookup(_queryFactory).CollectAsync(), [..lookup.Fields]))
					.FirstOrDefault();
            }

			return persisted;
		}
		public async Task<Data.Entities.User?> RetrieveUserAsync(String? id, String? email)
		{
			// Ανακτήστε τον χρήστη με βάση το ID.
			if (!String.IsNullOrEmpty(id))
				return await _userRepository.FindAsync(user => user.Id == id);

			// Ανακτήστε τον χρήστη με βάση το email.
			else if (!String.IsNullOrEmpty(email))
				return await _userRepository.FindAsync(user => user.Email == email);

			return null;
		}

		public String ExtractUserCredential(Data.Entities.User user)
		{
			switch (user.AuthProvider)
			{
				case AuthProvider.Local: return user.Password;
                case AuthProvider.Google: return user.AuthProviderId;
                default: throw new InvalidOperationException("Invalid user data given to extract user credential");
			}
		}

		// Συνάρτηση για hasing των credentials συνθηματικού του χρήστη
		private void HashLoginCredentials(ref Data.Entities.User user)
		{
			switch (user.AuthProvider)
			{
				case AuthProvider.Local:
					user.Password = Security.HashValue(user.Password);
					break;
                case AuthProvider.Google:
					user.AuthProviderId = Security.HashValue(user.AuthProviderId);
					break;
                default:
                    throw new InvalidOperationException("Invalid user data given to hash user credentials");
            }
		}

		public async Task<Models.User.User?> Get(String id, List<String> fields)
		{
			UserLookup lookup = new UserLookup();
			lookup.Ids = new List<String> { id };
			lookup.Fields = fields;
			lookup.PageSize = 1;
			lookup.Offset = 0;
            lookup.Fields = fields;
            List<Data.Entities.User> user = await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.None).CollectAsync();

			if (user == null)
				throw new NotFoundException("No user found with this id");

			return (await _builderFactory.Builder<UserBuilder>().Authorise(AuthorizationFlags.None).Build(user, fields)).FirstOrDefault();
		}

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			if (!await _authorizationService.AuthorizeOrOwnedAsync(_authorizationContentResolver.BuildOwnedResource(new UserLookup(), ids), Permission.DeleteUsers))
                throw new ForbiddenException("Unauthorised access", typeof(Data.Entities.User), Permission.DeleteUsers);

			UserLookup userLookup = new UserLookup();
			userLookup.Ids = ids;
			userLookup.Offset = 0;
            userLookup.PageSize = 10000;
			userLookup.Fields = new List<String>()
			{
				nameof(Models.User.User.Id),
				String.Join('.', nameof(Models.User.User.Shelter), nameof(Models.Shelter.Shelter.Id)),
                String.Join('.', nameof(Models.User.User.ProfilePhoto), nameof(Models.File.File.Id)),
            };

			List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).CollectAsync();

			await _shelterService.Value.Delete([..users.Where(user => !String.IsNullOrEmpty(user.ShelterId)).Select(user => user.ShelterId)]);

            await _shelterService.Value.Delete([.. users.Where(user => !String.IsNullOrEmpty(user.ShelterId)).Select(user => user.ShelterId)]);
            
			await _fileService.Value.Delete([.. users.Where(user => !String.IsNullOrEmpty(user.ProfilePhotoId)).Select(user => user.ProfilePhotoId)]);

            await _userRepository.DeleteManyAsync(ids);
		}
	}
}