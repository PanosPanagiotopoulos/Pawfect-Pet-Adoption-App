using Pawfect_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RateLimitAttribute : Attribute
    {
        public RateLimitLevel Level { get; }
        public String? CustomKey { get; }

        public RateLimitAttribute(RateLimitLevel level, String? customKey = null)
        {
            Level = level;
            CustomKey = customKey;
        }
    }
}
