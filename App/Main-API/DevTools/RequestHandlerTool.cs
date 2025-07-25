﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

namespace Main_API.DevTools
{
    /// <summary>
    /// Μια κλάση που παρέχει μεθόδους και εργαλεία για την
    /// χειρισμό διαφόρων προβλημάτων του API.
    /// </summary>
    public class RequestHandlerTool
    {
        /// <summary>
        /// Χειρίζεται το εσωτερικό σφάλμα του διακομιστή.
        /// </summary>
        /// <param name="error">Το σφάλμα.</param>
        /// <param name="method">Η μέθοδος από την οποία προέκυψε.</param>
        /// <param name="filepath">Το αρχείο που προέκυψε.</param>
        /// <param name="extraInfo">Πρόσθετες πληροφορίες για την ανατροφοδότηση.</param>
        /// <returns>IActionResult για την επστροφή σε request</returns>
        public static IActionResult HandleInternalServerError(Exception error, String method = "< Δεν περιλαμβάνεται στο μήνυμα >", String filepath = "< Δεν περιλαμβάνεται στο μήνυμα >", String extraInfo = "< Δεν περιλαμβάνεται >")
        {
            String errorMessage = $"\n---------------------------------------------------------\nΠροέκυψε εσωτερικό σφάλμα του διακομιστή κατά την επεξεργασία του αιτήματος.\nΑιτία: ${error.Message}\nΑνίχνευση: {error.StackTrace}\nΕσωτερική Εξαίρεση: {error.InnerException}\nΔεδομένα: {JsonConvert.SerializeObject(error.Data, Formatting.Indented)}\nΠρόσθετες πληροφορίες: ${extraInfo}\n---------------------------------------------------------\n";
            Log.Error(error, errorMessage);
            return new ObjectResult(errorMessage)
            {
                StatusCode = 500
            };
        }
    }
}