using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;

namespace Pawfect_Pet_Adoption_App_API.Services
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
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IMapper mapper
                           , ILogger<UserService> logger, IMemoryCache memoryCache
                           , ISmsService smsService, IEmailService emailService
                           , RequestService requestService, IConfiguration configuration)
        {
            this._userRepository = userRepository;
            this._mapper = mapper;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._smsService = smsService;
            this._emailService = emailService;
            this._requestService = requestService;
            this._configuration = configuration;
        }

        public async Task<string?> RegisterUserUnverifiedAsync(RegisterPersist registerPersist)
        {
            // Ελέγξτε αν ο χρήστης υπάρχει ήδη. Αν ναι, επιστρέψτε το υπάρχον ID του χρήστη.
            if (
                (await _userRepository.FindAsync(user => user.Email == registerPersist.User.Email))
                is User existingUser && existingUser != null
                )
            {
                return existingUser.Id;
            }

            // Χαρτογραφήστε τα δεδομένα του χρήστη και ορίστε τον χρήστη ως μη επιβεβαιωμένο.
            User user = _mapper.Map<User>(registerPersist.User);
            user.IsVerified = false;

            // Αποθηκεύστε τα δεδομένα του χρήστη.
            return await PersistUserAsync(user);
        }

        public async Task GenerateNewOtpAsync(string? phonenumber)
        {
            if (string.IsNullOrEmpty(phonenumber))
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

            if (!double.TryParse(_configuration["auth:timeInCache"], out double timeInCache))
            {
                // LOGS //
                _logger.LogError("Δεν βρέθηκε configuration για τον χρόνο στη cache");
                timeInCache = 15.0;
            }

            _memoryCache.Set(phonenumber, newOtp, TimeSpan.FromMinutes(timeInCache));

            // Στείλτε το OTP μέσω SMS.
            await _smsService.SendSmsAsync(phonenumber, string.Format(ISmsService.SmsTemplates[SmsType.OTP], newOtp));
        }

        public bool VerifyOtp(string? phonenumber, int? OTP)
        {
            if (string.IsNullOrEmpty(phonenumber))
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

        public async Task SendVerficationEmailAsync(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidDataException("Δεν βρέθηκε email για την αποστολή του email επιβεβαίωσης");
            }

            // Δημιουργήστε ένα νέο token επιβεβαίωσης email.
            string token = IEmailService.GenerateRefreshToken();

            // Αφαιρέστε το υπάρχον token αν υπάρχει.
            if (_memoryCache.TryGetValue(email, out _))
            {
                _memoryCache.Remove(email);
            }

            if (!double.TryParse(_configuration["auth:timeInCache"], out double timeInCache))
            {
                // LOGS //
                _logger.LogError("Δεν βρέθηκε configuration για τον χρόνο στη cache");
                timeInCache = 15.0;
            }

            // Αποθηκεύστε το νέο token στην cache.
            _memoryCache.Set(email, token, TimeSpan.FromMinutes(timeInCache));

            // Δημιουργήστε το URL επιβεβαίωσης.
            string verificationUrl = Path.Join(_requestService.GetBaseURI(), $"auth/verify-email?&token={token}");

            // Στείλτε το email επιβεβαίωσης.
            await _emailService.SendEmailAsync(email, EmailType.Verification.ToString(), string.Format(IEmailService.EmailTemplates[EmailType.Verification], verificationUrl));
        }

        public bool VerifyEmail(string? email, string? token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                throw new InvalidDataException("Δεν βρέθηκε email ή token για την επαλήθευση του email");
            }

            // Ελέγξτε αν το token επιβεβαίωσης email υπάρχει στην cache.
            if (!_memoryCache.TryGetValue(email, out string? refreshToken))
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

        public async Task<bool> VerifyUserAsync(RegisterPersist toRegisterUser)
        {
            try
            {
                // Ανακτήστε τον χρήστη με βάση το ID ή το email.
                User? user = await RetrieveUserAsync(toRegisterUser.User.Id, toRegisterUser.User.Email);

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

                return true;
            }
            catch (Exception e)
            {
                // LOGS //
                _logger.LogError($"Αποτυχία επιβαιβαίωσης χρήστη με σφάλμα:\n{JsonHelper.SerializeObjectFormatted(e)}");
                return false;
            }
        }

        public async Task SendResetPasswordEmailAsync(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidDataException("Δεν βρέθηκε email για την αποστολή του reset email");
            }

            // Κατασκευή καινούριο token
            string token = IEmailService.GenerateRefreshToken();

            // Αφαιρούμε προηγούμενη ενέργεια του χρήστη για επαναφορά
            if (_memoryCache.TryGetValue(email, out _))
            {
                _memoryCache.Remove(email);
            }

            // Παίρνουμε τον χρόνο που θα δοθεί για την επαναφορά
            if (!double.TryParse(_configuration["auth:timeInCache"], out double timeInCache))
            {
                // LOGS //
                _logger.LogError("Δεν βρέθηκε ο χρόνος στην cache στο configuration");
                timeInCache = 15.0;
            }

            // Store the new token in cache
            _memoryCache.Set(email, token, TimeSpan.FromMinutes(timeInCache));

            // Δημιουργία reset password URL
            string resetPasswordUrl = Path.Join(_requestService.GetBaseURI(), $"auth/reset-password?&token={token}");

            // Κατασκευή θέματος URL. Το σπάμε με κενό για καλύτερο projection στον χρήστη
            string subject = string.Join(' ', EmailType.Reset_Password.ToString().Split('_'));
            // Αποστολή reset password email
            await _emailService.SendEmailAsync(email, subject, string.Format(IEmailService.EmailTemplates[EmailType.Verification], resetPasswordUrl));
        }

        public async Task<bool> ResetPasswordAsync(string? email, string? password, string? token)
        {
            // Επαλήθευση email parameter
            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidDataException("Email είναι απαραίτητο για την επαναφορά του κωδικού.");
            }

            // Επαλήθευση password parameter
            if (string.IsNullOrEmpty(password))
            {
                throw new InvalidDataException("Ο νέος κωδικός είναι απαραίτητος.");
            }

            // Επαλήθευση token parameter
            if (string.IsNullOrEmpty(token))
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

                UserPersist newPasswordUser = new UserPersist()
                {
                    Email = email,
                    Password = password
                };

                // Κάνουμε update τον χρήστη με τον νέο κωδικό
                await PersistUserAsync(newPasswordUser, false);

                return true;
            }
            catch (Exception e)
            {
                // LOGS //
                _logger.LogError(e, "Αποτυχία επαναφοράς κωδικού");
                return false;
            }

        }

        public async Task<string?> PersistUserAsync(UserPersist userPersist, bool allowCreation = true)
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
                workingUser.UpdatedAt = DateTime.UtcNow;
                return await _userRepository.UpdateAsync(workingUser);
            }
            catch (Exception e)
            {
                // LOGS //
                _logger.LogError(e, "Σφάλμα στο persisting του χρήστη");
                return null;
            }
        }

        public async Task<string?> PersistUserAsync(User user, bool allowCreation = true)
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

                    return await _userRepository.AddAsync(user);
                }

                // Ενημερώστε τον υπάρχοντα χρήστη.
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
        public async Task<User?> RetrieveUserAsync(string? id, string? email)
        {
            // Ανακτήστε τον χρήστη με βάση το ID.
            if (!string.IsNullOrEmpty(id))
            {
                return await _userRepository.FindAsync(user => user.Id == id);
            }
            // Ανακτήστε τον χρήστη με βάση το email.
            else if (!string.IsNullOrEmpty(email))
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
    }
}