﻿using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
    public class ShelterQuery : BaseQuery<Shelter>
    {
        // Κατασκευαστής για την κλάση ShelterQuery
        // Είσοδος: mongoDbService - μια έκδοση της κλάσης MongoDbService
        public ShelterQuery(MongoDbService mongoDbService)
        {
            base._collection = mongoDbService.GetCollection<Shelter>();
        }

        // Λίστα με τα IDs των καταφυγίων για φιλτράρισμα
        public List<string>? Ids { get; set; }

        // Λίστα με τα IDs των χρηστών για φιλτράρισμα
        public List<string>? UserIds { get; set; }

        // Λίστα με τις καταστάσεις επιβεβαίωσης για φιλτράρισμα
        public List<VerificationStatus>? VerificationStatuses { get; set; }

        // Λίστα με τα IDs των admin που επιβεβαίωσαν για φιλτράρισμα
        public List<string>? VerifiedBy { get; set; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Shelter> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        protected override Task<FilterDefinition<Shelter>> ApplyFilters()
        {
            FilterDefinitionBuilder<Shelter> builder = Builders<Shelter>.Filter;
            FilterDefinition<Shelter> filter = builder.Empty;

            // Εφαρμόζει φίλτρο για τα IDs των καταφυγίων
            if (Ids != null && Ids.Any())
            {
                filter &= builder.In(shelter => shelter.Id, Ids);
            }

            // Εφαρμόζει φίλτρο για τα IDs των χρηστών
            if (UserIds != null && UserIds.Any())
            {
                filter &= builder.In(shelter => shelter.UserId, UserIds);
            }

            // Εφαρμόζει φίλτρο για τις καταστάσεις επιβεβαίωσης
            if (VerificationStatuses != null && VerificationStatuses.Any())
            {
                filter &= builder.In(shelter => shelter.VerificationStatus, VerificationStatuses);
            }

            // Εφαρμόζει φίλτρο για τα IDs των admin που επιβεβαίωσαν
            if (VerifiedBy != null && VerifiedBy.Any())
            {
                filter &= builder.In(shelter => shelter.VerifiedBy, VerifiedBy);
            }

            // Εφαρμόζει φίλτρο για fuzzy search μέσω indexing στη Mongo σε πεδίο : ShelterName
            if (!string.IsNullOrEmpty(Query))
            {
                filter &= builder.Text(Query);
            }

            return Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<string> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<string> FieldNamesOf(List<string> fields)
        {
            if (fields == null) return new List<string>();
            if (fields.Any() || fields.Contains("*")) return EntityHelper.GetAllPropertyNames(typeof(Shelter)).ToList();

            HashSet<string> projectionFields = new HashSet<string>();
            foreach (string item in fields)
            {
                // Αντιστοιχίζει τα ονόματα πεδίων ShelterDto στα ονόματα πεδίων Shelter
                if (item.Equals(nameof(ShelterDto.Id))) projectionFields.Add(nameof(Shelter.Id));
                if (item.StartsWith(nameof(ShelterDto.User))) projectionFields.Add(nameof(Shelter.UserId));
                if (item.Equals(nameof(ShelterDto.ShelterName))) projectionFields.Add(nameof(Shelter.ShelterName));
                if (item.Equals(nameof(ShelterDto.Description))) projectionFields.Add(nameof(Shelter.Description));
                if (item.Equals(nameof(ShelterDto.Website))) projectionFields.Add(nameof(Shelter.Website));
                if (item.Equals(nameof(ShelterDto.SocialMedia))) projectionFields.Add(nameof(Shelter.SocialMedia));
                if (item.Equals(nameof(ShelterDto.OperatingHours))) projectionFields.Add(nameof(Shelter.OperatingHours));
                if (item.Equals(nameof(ShelterDto.VerificationStatus))) projectionFields.Add(nameof(Shelter.VerificationStatus));
                if (item.Equals(nameof(ShelterDto.VerifiedBy))) projectionFields.Add(nameof(Shelter.VerifiedBy));
            }
            return projectionFields.ToList();
        }
    }
}