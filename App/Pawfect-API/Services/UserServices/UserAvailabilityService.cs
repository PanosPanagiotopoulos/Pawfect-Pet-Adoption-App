using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Models.UserAvailability;

namespace Pawfect_API.Services.UserServices
{
    public class UserAvailabilityService: IUserAvailabilityService
    {
        private readonly ILogger<UserAvailabilityService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IUserRepository _userRepository;

        public UserAvailabilityService
        (
            ILogger<UserAvailabilityService> logger,
            IMemoryCache memoryCache,
            IUserRepository userRepository
        )
        {
            this._logger = logger;
            this._memoryCache = memoryCache;
            this._userRepository = userRepository;
        }
        public async Task<UserAvailabilityResult> CheckUserAvailabilityAsync(UserAvailabilityCheck availabilityCheck)
        {
            if (String.IsNullOrEmpty(availabilityCheck.Email) && String.IsNullOrEmpty(availabilityCheck.Phone)) throw new ArgumentException("Either email or phone must be provided");

            UserAvailabilityResult result = new UserAvailabilityResult();

            String? emailCacheKey = !String.IsNullOrEmpty(availabilityCheck.Email) ? $"email_available_{availabilityCheck.Email.ToLowerInvariant()}" : null;
            String? phoneCacheKey = !String.IsNullOrEmpty(availabilityCheck.Phone) ? $"phone_available_{availabilityCheck.Phone}" : null;

            Boolean emailFromCache = false, phoneFromCache = false;

            // Check email cache
            if (!String.IsNullOrEmpty(emailCacheKey))
            {
                if (_memoryCache.TryGetValue(emailCacheKey, out Boolean emailAvailable))
                {
                    result.IsEmailAvailable = emailAvailable;
                    if (!emailAvailable) result.EmailMessage = "This email address is already registered";
                    emailFromCache = true;
                }
            }

            // Check phone cache
            if (!String.IsNullOrEmpty(phoneCacheKey))
            {
                if (_memoryCache.TryGetValue(phoneCacheKey, out Boolean phoneAvailable))
                {
                    result.IsPhoneAvailable = phoneAvailable;
                    if (!phoneAvailable) result.PhoneMessage = "This phone number is already registered";
                    phoneFromCache = true;
                }
            }

            // If both values are cached, return immediately
            if ((String.IsNullOrEmpty(availabilityCheck.Email) || emailFromCache) &&
                (String.IsNullOrEmpty(availabilityCheck.Phone) || phoneFromCache))
            {
                return result;
            }

            List<Task> dbTasks = new List<Task>();

            if (!String.IsNullOrEmpty(availabilityCheck.Email) && !emailFromCache)
            {
                dbTasks.Add(this.CheckAndCacheEmailAvailabilityAsync(availabilityCheck.Email, result));
            }

            if (!String.IsNullOrEmpty(availabilityCheck.Phone) && !phoneFromCache)
            {
                dbTasks.Add(this.CheckAndCachePhoneAvailabilityAsync(availabilityCheck.Phone, result));
            }

            if (dbTasks.Any())
            {
                await Task.WhenAll(dbTasks);
            }

            return result;
        }

        private async Task CheckAndCacheEmailAvailabilityAsync(String email, UserAvailabilityResult result)
        {
            String cacheKey = $"email_available_{email.ToLowerInvariant()}";

            try
            {
                Boolean isAvailable = !await _userRepository.ExistsAsync(user => user.Email.Equals(email));
                result.IsEmailAvailable = isAvailable;

                if (!isAvailable)
                {
                    result.EmailMessage = "This email address is already registered";
                    // Cache unavailable emails longer since they're less likely to change
                    _memoryCache.Set(cacheKey, false, TimeSpan.FromMinutes(10));
                }
                else
                {
                    // Cache available emails for shorter time since they might get registered
                    _memoryCache.Set(cacheKey, true, TimeSpan.FromMinutes(8));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email availability for {Email}", email);
                // Fail safe - don't cache errors, return as available to not block users
                result.IsEmailAvailable = true;
            }
        }

        private async Task CheckAndCachePhoneAvailabilityAsync(String phone, UserAvailabilityResult result)
        {
            String cacheKey = $"phone_available_{phone}";
            try
            {
                Boolean isAvailable = !await _userRepository.ExistsAsync(user => user.Phone.Equals(phone));
                result.IsPhoneAvailable = isAvailable;

                if (!isAvailable)
                {
                    result.PhoneMessage = "This phone number is already registered";
                    _memoryCache.Set(cacheKey, false, TimeSpan.FromMinutes(10));
                }
                else
                {
                    _memoryCache.Set(cacheKey, true, TimeSpan.FromMinutes(8));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking phone availability for {Phone}", phone);
                result.IsPhoneAvailable = true; 
            }
        }
        public void InvalidateAvailabilityCache(String email, String phone)
        {
            if (!String.IsNullOrEmpty(email))
            {
                _memoryCache.Remove($"email_available_{email.ToLowerInvariant()}");
            }

            if (!String.IsNullOrEmpty(phone))
            {
                _memoryCache.Remove($"phone_available_{phone}");
            }
        }
    }
}
