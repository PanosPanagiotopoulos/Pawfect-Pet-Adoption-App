using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Pawfect_Pet_Adoption_App_API.DevTools
{
    /// <summary>
    /// A class for that provides methods and tools for 
    /// handling different API problems.
    /// </summary>
    public class RequestHandlerTool
    {
        /// <summary>
        /// Handles the internal server error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="method">The method that was occured from.</param>
        /// <param name="filepath">The filepath that was occured from.</param>
        /// <param name="extraInfo">Extra information for the feedback.</param>
        /// <returns></returns>
        public static IActionResult HandleInternalServerError(Exception error, string method, string filepath = "< Not included >", string extraInfo = "")
        {
            string errorMessage = $"\n---------------------------------------------------------\nInternal server error occurred while processing the request.\nCause: ${error.Message}\nTrace: {error.StackTrace}\nInner Exception: {error.InnerException}\nData: {JsonConvert.SerializeObject(error.Data, Formatting.Indented)}\nExtra info: ${extraInfo}\n---------------------------------------------------------\n";
            Console.WriteLine(errorMessage);
            return new ObjectResult(errorMessage)
            {
                StatusCode = 500
            };
        }
    }
}