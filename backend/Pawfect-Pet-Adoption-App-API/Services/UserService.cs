using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
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

        public UserService(IUserRepository userRepository, IMapper mapper
                           , ILogger<UserService> logger, IMemoryCache memoryCache
                           , ISmsService smsService, IEmailService emailService)
        {
            this._userRepository = userRepository;
            this._mapper = mapper;
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._smsService = smsService;
            this._emailService = emailService;
        }

        public async Task<string> RegisterUserUnverifiedAsync(RegisterPersist registerPersist)
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
            return await PersistUser(user);
        }

        public async Task GenerateNewOtp(string phonenumber)
        {
            // Αφαιρέστε το υπάρχον OTP αν υπάρχει.
            if (_memoryCache.TryGetValue<int>(phonenumber, out _))
            {
                _memoryCache.Remove(phonenumber);
            }

            // Δημιουργήστε ένα νέο OTP και αποθηκεύστε το στην cache.
            int newOtp = ISmsService.GenerateOtp();
            _memoryCache.Set(phonenumber, newOtp, TimeSpan.FromMinutes(10));

            // Στείλτε το OTP μέσω SMS.
            await _smsService.SendSmsAsync(phonenumber, $"Your OTP is {newOtp}");
        }

        public bool VerifyOtp(string phonenumber, OTPVerification otpVerification)
        {
            // Ελέγξτε αν το OTP υπάρχει στην cache.
            if (!_memoryCache.TryGetValue(phonenumber, out int cachedOtp))
            {
                throw new InvalidDataException("Δεν έχει αποσταλθεί μήνυμα OTP σε αυτόν τον αριθμό τηλεφώνου.");
            }

            // Επαληθεύστε το OTP.
            if (!(cachedOtp == otpVerification.Otp))
            {
                throw new InvalidDataException("Λάθος κωδικός OTP");
            }

            return true;
        }

        public async Task SendVerficationEmailAsync(string email)
        {
            // Δημιουργήστε ένα νέο token επιβεβαίωσης email.
            string token = IEmailService.GenerateRefreshToken();

            // Αφαιρέστε το υπάρχον token αν υπάρχει.
            if (_memoryCache.TryGetValue(email, out _))
            {
                _memoryCache.Remove(email);
            }

            // Αποθηκεύστε το νέο token στην cache.
            _memoryCache.Set(email, token, TimeSpan.FromMinutes(15));

            // Δημιουργήστε το URL επιβεβαίωσης.
            string verificationUrl = $"https://localhost:7286/verify-email?&token={token}";

            // Στείλτε το email επιβεβαίωσης.
            await _emailService.SendEmailAsync(email, "Email Verification", $"Please verify your email by clicking on this link: <a>{verificationUrl}</a>");
        }

        public bool VerifyEmail(string email, string token)
        {
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

            return true;
        }

        public async Task<string?> PersistUser(UserPersist userPersist)
        {
            User workingUser = null;
            try
            {
                // Ανακτήστε τον χρήστη με βάση το ID ή το email.
                workingUser = await RetrieveUser(userPersist.Id, userPersist.Email);

                // Δημιουργήστε έναν νέο χρήστη αν δεν υπάρχει.
                if (workingUser == null)
                {
                    workingUser = _mapper.Map<User>(userPersist);
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
                // Καταγράψτε το σφάλμα.
                _logger.LogError(e, "Σφάλμα στο persisting του χρήστη");
                return null;
            }
        }

        public async Task<string?> PersistUser(User user)
        {
            User workingUser = null;
            try
            {
                // Ανακτήστε τον χρήστη με βάση το ID ή το email.
                workingUser = await RetrieveUser(user.Id, user.Email);

                // Δημιουργήστε έναν νέο χρήστη αν δεν υπάρχει.
                if (workingUser == null)
                {
                    workingUser = _mapper.Map<User>(user);
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

        public async Task<bool> VerifyUser(RegisterPersist toRegisterUser)
        {
            try
            {
                // Ανακτήστε τον χρήστη με βάση το ID ή το email.
                User user = await RetrieveUser(toRegisterUser.User.Id, toRegisterUser.User.Email);

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
                    _logger.LogError("Ο χρήστη δεν επιβεβαιώθηκε λόγο απουσίας validation κριτηρίων\n" +
                                     $"Κριτήρια: Αριθμός Τηλεφώνου : {user.HasPhoneVerified}" +
                                     $"Κριτήρια: Email : {user.HasEmailVerified}");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                // Καταγράψτε το σφάλμα.
                _logger.LogError($"Αποτυχία επιβαιβαίωσης χρήστη με σφάλμα:\n{JsonHelper.SerializeObjectFormatted(e)}");
                return false;
            }
        }

        public async Task<User?> RetrieveUser(string id, string email)
        {
            // Ανακτήστε τον χρήστη με βάση το ID.
            if (!string.IsNullOrEmpty(id))
            {
                return await _userRepository.GetByIdAsync(id);
            }
            // Ανακτήστε τον χρήστη με βάση το email.
            else if (!string.IsNullOrEmpty(email))
            {
                return await _userRepository.FindAsync(user => user.Email == email);
            }

            return null;
        }
    }
}