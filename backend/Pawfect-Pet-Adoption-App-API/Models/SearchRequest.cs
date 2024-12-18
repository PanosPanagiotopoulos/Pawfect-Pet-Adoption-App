﻿namespace Pawfect_Pet_Adoption_App_API.Models
{
    public class SearchRequest
    {
        // Το query προς αναζήτηση στον search server
        public string Query { get; set; }
        // Το πλήθος των αποτελεσμάτων που θα επιστραφούν
        public int TopK { get; set; } = 5;
        // Η γλώσσα των αποτελεσμάτων και η γλώσσα στην οποία θα εξεταστεί το query
        public string Lang { get; set; }
    }
}
