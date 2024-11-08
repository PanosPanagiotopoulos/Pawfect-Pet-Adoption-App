namespace Pawfect_Pet_Adoption_App_API.DevTools
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System.Text.Json;

    namespace Pawfect_Pet_Adoption_App_API.DevTools
    {
        public class JsonExceptionFilter : IExceptionFilter
        {
            public void OnException(ExceptionContext context)
            {
                if (context.Exception is JsonException)
                {
                    context.Result = new BadRequestObjectResult(new { Error = context.Exception.Message });
                    context.ExceptionHandled = true;
                }
            }
        }
    }
}
