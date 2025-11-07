using InternalSystem_ModelContext;
using InternalSystem_ModelContext.Models.SQLite;
using System.Linq;
using System.Net;
using THMY_API.Controllers;
using THMY_API.Models;
using THMY_API.Models.DBContext;

namespace THMY_API.Middlewares
{
    public class APIKeyMiddleware(RequestDelegate next)
    {
        //private readonly APIContext _context = apiContext;

        public async Task InvokeAsync(HttpContext context, APIContext apiContext)
        {

            var allowedPaths = new List<string> { "/openapi", "/swagger", "/scalar", "/favicon" };
            //if (allowedPaths.Contains(context.Request.Path))
            if(allowedPaths.Where(p => context.Request.Path.ToString().StartsWith(p)).FirstOrDefault() != null)
            {
                // Skip API key check for these paths
                await next(context);
                return;
            }

            APIStorage api;

            //check for system name;
            if (!context.Request.Headers.TryGetValue("Application-Name", out var applicationName))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Application Name missing");
                return;
            }

            if (apiContext.APIStroageKey.Any(a => a.applicationName.Equals(applicationName)) == false)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Application Name isn't registered");
                return;
            }

            api = apiContext.APIStroageKey.First(a => a.applicationName.Equals(applicationName));

            //check api key;
            if (!context.Request.Headers.TryGetValue("API-Key", out var apiKey))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("API key missing");
                return;
            }

            if (apiKey.Equals(api.apiSecret) == false)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("API Key is invalid");
                return;
            }

            await next(context);

        }
    }
}
