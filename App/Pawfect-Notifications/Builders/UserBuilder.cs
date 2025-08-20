using AutoMapper;
using Pawfect_Notifications.Data.Entities.EnumTypes;
using Pawfect_Notifications.Data.Entities.Types.Authorization;
using Pawfect_Notifications.Query;
using Pawfect_Notifications.Services.AuthenticationServices;
using System.Security.Claims;
namespace Pawfect_Notifications.Builders
{
	public class AutoUserBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : User
		public AutoUserBuilder()
		{
            // Mapping για το Entity : User σε User για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Data.Entities.User, Data.Entities.User>();
			CreateMap<Models.User.User, Data.Entities.User>().ReverseMap();
        }
	}
	public class UserBuilder : BaseBuilder<Models.User.User, Data.Entities.User>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;

        public UserBuilder
		(
			IQueryFactory queryFactory, 
            IBuilderFactory builderFactory,
            IAuthorizationContentResolver authorizationContentResolver,
            ClaimsExtractor claimsExtractor
        )
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
            this._authorizationContentResolver = authorizationContentResolver;
            this._claimsExtractor = claimsExtractor;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        
        public UserBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<Models.User.User>> Build(List<Data.Entities.User> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = this.ExtractBuildFields(fields);

            List<Models.User.User> result = new List<Models.User.User>();
			foreach (Data.Entities.User e in entities)
			{
                Models.User.User dto = new Models.User.User();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Models.User.User.Email))) dto.Email = e.Email;
				if (nativeFields.Contains(nameof(Models.User.User.FullName))) dto.FullName = e.FullName;
				
				if (nativeFields.Contains(nameof(Models.User.User.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Models.User.User.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;

                result.Add(dto);
			}

			return await Task.FromResult(result);
		}
    }
}
