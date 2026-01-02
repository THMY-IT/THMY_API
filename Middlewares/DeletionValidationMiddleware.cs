using Microsoft.AspNetCore.Http;
using System.Text.Json;
using THMY_API.Models;
using THMY_API.Services;

namespace THMY_API.Middlewares
{
    /// <summary>
    /// Middleware that intercepts DELETE requests and validates if the entity can be deleted
    /// by checking for dependencies in related tables. This prevents orphaned references
    /// in the database when entities don't have explicit foreign key relationships.
    /// </summary>
    public class DeletionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DeletionValidationMiddleware> _logger;

        public DeletionValidationMiddleware(RequestDelegate next, ILogger<DeletionValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Processes the HTTP request and validates deletion requests before they reach the controller.
        /// </summary>
        /// <param name="context">The HTTP context for the current request</param>
        /// <param name="serviceProvider">Service provider to resolve scoped dependencies</param>
        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            // Only intercept DELETE requests
            if (context.Request.Method == HttpMethods.Delete)
            {
                var path = context.Request.Path.Value?.ToLower();

                // Check which entity is being deleted and validate accordingly
                if (path?.Contains("/permission/") == true)
                {
                    await HandlePermissionDeletion(context, serviceProvider);
                    return;
                }
                else if (path?.Contains("/role/") == true && !path.Contains("/rolepermission"))
                {
                    await HandleRoleDeletion(context, serviceProvider);
                    return;
                }
            }

            // If not a DELETE request or validation passed, continue to next middleware
            await _next(context);
        }

        /// <summary>
        /// Handles validation for Permission deletion.
        /// Checks if the permission is assigned to any roles via RolePermission table.
        /// </summary>
        private async Task HandlePermissionDeletion(HttpContext context, IServiceProvider serviceProvider)
        {
            var permissionId = ExtractIdFromPath(context.Request.Path, "permission");
            
            if (permissionId.HasValue)
            {
                // Create a new scope to resolve scoped services (like DbContext)
                using var scope = serviceProvider.CreateScope();
                var validator = scope.ServiceProvider.GetRequiredService<IDeletionValidator>();
                
                var result = await validator.CanDeletePermission(permissionId.Value);
                
                if (!result.CanDelete)
                {
                    _logger.LogWarning("DELETE request blocked for Permission ID {PermissionId}: {Reason}", 
                                      permissionId.Value, result.Reason);
                    await WriteErrorResponse(context, result);
                    return;
                }
            }

            await _next(context);
        }

        /// <summary>
        /// Handles validation for Role deletion.
        /// Checks if the role is assigned to any employees or has any permissions.
        /// </summary>
        private async Task HandleRoleDeletion(HttpContext context, IServiceProvider serviceProvider)
        {
            var roleId = ExtractIdFromPath(context.Request.Path, "role");
            
            if (roleId.HasValue)
            {
                using var scope = serviceProvider.CreateScope();
                var validator = scope.ServiceProvider.GetRequiredService<IDeletionValidator>();
                
                var result = await validator.CanDeleteRole(roleId.Value);
                
                if (!result.CanDelete)
                {
                    _logger.LogWarning("DELETE request blocked for Role ID {RoleId}: {Reason}", 
                                      roleId.Value, result.Reason);
                    await WriteErrorResponse(context, result);
                    return;
                }
            }

            await _next(context);
        }

        /// <summary>
        /// Extracts the entity ID from the request path.
        /// For example, from "/Permission/5" it extracts 5.
        /// </summary>
        /// <param name="path">The request path</param>
        /// <param name="entityName">The entity name to look for (e.g., "permission")</param>
        /// <returns>The extracted ID, or null if not found</returns>
        private int? ExtractIdFromPath(PathString path, string entityName)
        {
            var segments = path.Value?.Split('/');
            if (segments == null) return null;

            for (int i = 0; i < segments.Length - 1; i++)
            {
                if (segments[i].Equals(entityName, StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(segments[i + 1], out int id))
                    {
                        return id;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Writes an error response with HTTP 409 Conflict status when deletion is not allowed.
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <param name="result">The validation result containing error details and dependent data</param>
        private async Task WriteErrorResponse(HttpContext context, DeletionValidationResult result)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                error = "Deletion not allowed",
                message = result.Reason,
                dependencies = result.Dependencies,
                dependentData = result.DependentData,
                statusCode = 409
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
        }
    }
}

