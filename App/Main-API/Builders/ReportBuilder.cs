﻿using Amazon.Runtime;
using AutoMapper;

using Main_API.Data.Entities;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Models.Lookups;
using Main_API.Models.Report;
using Main_API.Models.User;
using Main_API.Query;
using Main_API.Services.UserServices;

namespace Main_API.Builders
{
	public class AutoReportBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : Report
		public AutoReportBuilder()
		{
            // Mapping για το Entity : Report σε Report για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Data.Entities.Report, Data.Entities.Report>();

            // POST Request Dto Μοντέλα
            CreateMap<Data.Entities.Report, ReportPersist>();
            CreateMap<ReportPersist, Data.Entities.Report>();
		}
	}

	public class ReportBuilder : BaseBuilder<Models.Report.Report, Data.Entities.Report>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        public ReportBuilder(IQueryFactory queryFactory, IBuilderFactory builderFactory)
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
       

        public ReportBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<Models.Report.Report>> Build(List<Data.Entities.Report> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

            List<String> userFields = new List<String>();
            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            if (foreignEntitiesFields.ContainsKey(nameof(Models.Report.Report.Reporter)))
                userFields.AddRange(foreignEntitiesFields[nameof(Models.Report.Report.Reporter)]);
            if (foreignEntitiesFields.ContainsKey(nameof(Models.Report.Report.Reported)))
                userFields.AddRange(foreignEntitiesFields[nameof(Models.Report.Report.Reported)]);

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<String, List<Models.User.User>>? userMap = userFields.Count > 0 ? await this.CollectUsers(entities, userFields) : null;


            List<Models.Report.Report> result = new List<Models.Report.Report>();
			foreach (Data.Entities.Report e in entities)
			{
                Models.Report.Report dto = new Models.Report.Report();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Models.Report.Report.Type))) dto.Type = e.Type;
				if (nativeFields.Contains(nameof(Models.Report.Report.Reason))) dto.Reason = e.Reason;
				if (nativeFields.Contains(nameof(Models.Report.Report.Status))) dto.Status = e.Status;
				if (nativeFields.Contains(nameof(Models.Report.Report.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Models.Report.Report.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
                if (userMap != null && userMap.TryGetValue(e.Id, out List<Models.User.User> reporters) && reporters != null && reporters.Count > 0) dto.Reporter = reporters[0];
                if (userMap != null && userMap.TryGetValue(e.Id, out List<Models.User.User> reportereds) && reportereds != null && reportereds.Count > 0) dto.Reported = reportereds[1];

                result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, List<Models.User.User>>> CollectUsers(List<Data.Entities.Report> reports, List<String> userFields)
		{
            if (reports.Count == 0 || userFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> userIds = [.. reports.SelectMany(x => new[] { x.ReporterId, x.ReportedId }).Distinct()];

            UserLookup userLookup = new UserLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            userLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            userLookup.PageSize = 10000;
            userLookup.Ids = userIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<Models.User.User> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).Build(users, userFields);

            if (userDtos == null || userDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<String, Models.User.User> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα reports δημιουργώντας ένα Dictionary : [ ReportId -> UserId ] 
			return reports.ToDictionary(x => x.Id, x => new List<Models.User.User> { userDtoMap[x.ReporterId], userDtoMap[x.ReportedId] });
		}
	}
}