using Microsoft.EntityFrameworkCore;
using THMY_API.Models;
using THMY_API.Models.DBContext;

namespace THMY_API.Services
{
    /// <summary>
    /// Implementation of the deletion validator service.
    /// Checks database tables for dependencies before allowing entity deletion.
    /// </summary>
    public class DeletionValidator : IDeletionValidator
    {
        private readonly APIContext _context;
        private readonly ILogger<DeletionValidator> _logger;

        public DeletionValidator(APIContext context, ILogger<DeletionValidator> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Checks if a Permission can be deleted by verifying it's not assigned to any roles.
        /// </summary>
        /// <param name="permissionId">The permission ID to check</param>
        /// <returns>Validation result with details about why deletion is/isn't allowed</returns>
        public async Task<DeletionValidationResult> CanDeletePermission(int permissionId)
        {
            var result = new DeletionValidationResult { CanDelete = true };

            try
            {
                // Check if permission is assigned to any roles via RolePermission table
                var rolePermissionCount = await _context.RolePermission
                    .Where(rp => rp.permissionId == permissionId)
                    .CountAsync();

                if (rolePermissionCount > 0)
                {
                    result.CanDelete = false;
                    result.Dependencies.Add($"{rolePermissionCount} Role-Permission assignment(s)");
                    
                    // Get the actual role objects that are using this permission
                    var affectedRoles = await _context.RolePermission
                        .Where(rp => rp.permissionId == permissionId)
                        .Join(_context.Role,
                              rp => rp.roleId,
                              r => r.roleId,
                              (rp, r) => new Role
                              {
                                  roleId = r.roleId,
                                  roleName   = r.roleName,
                                  roleDescription = r.roleDescription
                              })
                        .ToListAsync();

                    result.Reason = $"Cannot delete Permission (ID: {permissionId}). " +
                                   $"It is currently assigned to {rolePermissionCount} role(s): " +
                                   $"{string.Join(", ", affectedRoles.Select(r => r.roleName))}. " +
                                   $"Please remove these role-permission assignments first.";
                    
                    // Populate dependent data with actual objects
                    result.DependentData = new DependentData
                    {
                        roles = affectedRoles
                    };
                    
                    _logger.LogWarning("Deletion validation failed for Permission {PermissionId}: " +
                                      "Referenced by {Count} role(s)", permissionId, rolePermissionCount);
                }
                else
                {
                    _logger.LogDebug("Permission {PermissionId} can be safely deleted - no dependencies found", 
                                    permissionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating deletion for Permission {PermissionId}", permissionId);
                result.CanDelete = false;
                result.Reason = "An error occurred while validating the deletion request.";
                result.Dependencies.Add($"Validation error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Checks if a Role can be deleted by verifying it has no employee assignments or permissions.
        /// </summary>
        /// <param name="roleId">The role ID to check</param>
        /// <returns>Validation result with details about why deletion is/isn't allowed</returns>
        public async Task<DeletionValidationResult> CanDeleteRole(int roleId)
        {
            var result = new DeletionValidationResult { CanDelete = true };

            try
            {
                // Initialize dependent data
                var dependentData = new DependentData();
                
                // Check if role is assigned to any employees via EmpRole table
                var empRoleAssignments = await _context.EmpRole
                    .Where(er => er.roleId == roleId)
                    .ToListAsync();

                if (empRoleAssignments.Any())
                {
                    result.CanDelete = false;
                    result.Dependencies.Add($"{empRoleAssignments.Count} Employee-Role assignment(s)");
                    
                    // Get actual employee role details
                    dependentData.employeeRoles = empRoleAssignments.Select(er => new EmpRole
                    {
                        empId = er.empId,
                        roleId = er.roleId,
                        systemId = er.systemId,
                        createdAt = er.createdAt
                    }).ToList();
                }

                // Check if role has any permissions assigned via RolePermission table
                var rolePermissionAssignments = await _context.RolePermission
                    .Where(rp => rp.roleId == roleId)
                    .Join(_context.Permission,
                          rp => rp.permissionId,
                          p => p.permissionId,
                          (rp, p) => new RolePermission
                          {
                              roleId = rp.roleId,
                              permissionId = rp.permissionId,
                              systemId = rp.systemId,
                              createdAt = rp.createdAt
                          })
                    .ToListAsync();

                if (rolePermissionAssignments.Any())
                {
                    result.CanDelete = false;
                    result.Dependencies.Add($"{rolePermissionAssignments.Count} Role-Permission assignment(s)");
                    dependentData.rolePermissions = rolePermissionAssignments;
                }

                // Build detailed error message if deletion is not allowed
                if (!result.CanDelete)
                {
                    var reasonParts = new List<string>();
                    
                    if (empRoleAssignments.Any())
                    {
                        reasonParts.Add($"{empRoleAssignments.Count} employee(s) are assigned this role");
                    }
                    
                    if (rolePermissionAssignments.Any())
                    {
                        reasonParts.Add($"{rolePermissionAssignments.Count} permission(s) are assigned to this role");
                    }

                    result.Reason = $"Cannot delete Role (ID: {roleId}). " +
                                   string.Join(" and ", reasonParts) + ". " +
                                   "Please remove these assignments first.";
                    
                    result.DependentData = dependentData;
                    
                    _logger.LogWarning("Deletion validation failed for Role {RoleId}: {Reason}", 
                                      roleId, result.Reason);
                }
                else
                {
                    _logger.LogDebug("Role {RoleId} can be safely deleted - no dependencies found", roleId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating deletion for Role {RoleId}", roleId);
                result.CanDelete = false;
                result.Reason = "An error occurred while validating the deletion request.";
                result.Dependencies.Add($"Validation error: {ex.Message}");
            }

            return result;
        }
    }
}

