using AutoMapper;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using MongoDB.Driver;
using Newtonsoft.Json;
using Main_API.Builders;
using Main_API.Censors;
using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Apis;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Data.Entities.Types.Cache;
using Main_API.DevTools;
using Main_API.Exceptions;
using Main_API.Models;
using Main_API.Models.File;
using Main_API.Models.Lookups;
using Main_API.Models.User;
using Main_API.Query;
using Main_API.Repositories.Interfaces;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.EmailServices;
using Main_API.Services.FileServices;
using Main_API.Services.HttpServices;
using Main_API.Services.ShelterServices;
using Main_API.Services.SmsServices;
using System.Security.Claims;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Main_API.Services.Convention;

namespace Main_API.Services.UserServices
{
	public class UserService : IUserService
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;
		private readonly ILogger<UserService> _logger;
		private readonly IMemoryCache _memoryCache;
		private readonly ISmsService _smsService;
		private readonly IEmailService _emailService;
		private readonly RequestService _requestService;
		private readonly CacheConfig _cacheConfig;
		private readonly Lazy<IShelterService> _shelterService;
		private readonly IAuthenticationService _authenticationService;
		private readonly Lazy<IFileService> _fileService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly IConventionService _conventionService;

        public UserService
		(
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
            IUserRepository userRepository, IMapper mapper,
			ILogger<UserService> logger, IMemoryCache memoryCache,
			ISmsService smsService, IEmailService emailService,
			RequestService requestService,
			IOptions<CacheConfig> configuration,
			Lazy<IShelterService> shelterService,
			IAuthenticationService authenticationService,
			Lazy<IFileService> fileService,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
			ClaimsExtractor claimsExtractor,
			IAuthorizationService AuthorizationService,
			IAuthorizationContentResolver authorizationContentResolver,
            IConventionService conventionService
        )
		{
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _userRepository = userRepository;
			_mapper = mapper;
			_logger = logger;
			_memoryCache = memoryCache;
			_smsService = smsService;
			_emailService = emailService;
			_requestService = requestService;
			_cacheConfig = configuration.Value;
			_shelterService = shelterService;
			_authenticationService = authenticationService;
			_fileService = fileService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _claimsExtractor = claimsExtractor;
            _authorizationService = AuthorizationService;
            _authorizationContentResolver = authorizationContentResolver;
            _conventionService = conventionService;
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

		public async Task GenerateNewOtpAsync(String phonenumber)
		{
			if (String.IsNullOrEmpty(phonenumber))
				throw new ArgumentException("No phone number found");

			// Αφαιρέστε το υπάρχον OTP αν υπάρχει.
			if (_memoryCache.TryGetValue<int>(phonenumber, out _))
				_memoryCache.Remove(phonenumber);

			// Δημιουργήστε ένα νέο OTP και αποθηκεύστε το στην cache.
			int newOtp = ISmsService.GenerateOtp();

			_memoryCache.Set(phonenumber, newOtp, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));

			// Στείλτε το OTP μέσω SMS.
			await _smsService.SendSmsAsync(phonenumber, String.Format(ISmsService.SmsTemplates[SmsType.OTP], newOtp));
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
			if (String.IsNullOrEmpty(email))
				throw new ArgumentException("No email found to send verification email");

			// Δημιουργήστε ένα νέο token επιβεβαίωσης email.
			String token = IEmailService.GenerateRefreshToken();

			// Αποθηκεύστε το νέο token στην cache.
			_memoryCache.Set(token, email, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));

			// Δημιουργήστε το URL επιβεβαίωσης.
			String verificationUrl = Path.Join(_requestService.GetFrontendBaseURI(), $"auth/verified?token={token}");

			String firstname = (await _userRepository.FindAsync(user => user.Email == email, [nameof(Data.Entities.User.FullName)]))?.FullName?.Split(" ")[0];
			if (String.IsNullOrEmpty(firstname)) throw new NotFoundException("User was not found to send email");

            // Create the template with the appropriate parameters
            Dictionary<String, String> parameters = new Dictionary<String, String>
            {
                { "Firstname", firstname },
                { "VerificationLink", verificationUrl }
            };

            String message = await _emailService.GetEmailTemplateAsync(EmailType.Verification, parameters);

            // Send email
            await _emailService.SendEmailAsync(email, EmailType.Verification.ToString(), message);
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
				// TODO: Στείλτε ειδοποίηση στον admin για να επιβεβαιώσει τον χρήστη
				user.IsVerified = false;

			}

			await Persist(user, false, buildDto: false);

			return true;
		}

		public async Task SendResetPasswordEmailAsync(String email)
		{
			if (String.IsNullOrEmpty(email))
				throw new ArgumentException("No email found to send reset password email");

			// Κατασκευή καινούριο token
			String token = IEmailService.GenerateRefreshToken();

			// Store the new token in cache
			_memoryCache.Set(token, email, TimeSpan.FromMinutes(_cacheConfig.TokensCacheTime));

			// Δημιουργία reset password URL
			String resetPasswordUrl = Path.Join(_requestService.GetFrontendBaseURI(), $"auth/reset-password?token={token}");

			// Κατασκευή θέματος URL. Το σπάμε με κενό για καλύτερο projection στον χρήστη
			String subject = String.Join(' ', EmailType.Reset_Password.ToString().Split('_'));

            String firstname = (await _userRepository.FindAsync(user => user.Email == email, [nameof(Data.Entities.User.FullName)]))?.FullName?.Split(" ")[0];
            if (String.IsNullOrEmpty(firstname)) throw new NotFoundException("User was not found to send email");

            Dictionary<String, String> parameters = new Dictionary<String, String>
            {
                { "Firstname", firstname },
                { "ResetLink", resetPasswordUrl }
            };

            String message = await _emailService.GetEmailTemplateAsync(EmailType.Reset_Password, parameters);

            // Αποστολή reset password email
            await _emailService.SendEmailAsync(email, subject, message);
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