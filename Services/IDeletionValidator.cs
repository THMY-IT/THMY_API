using THMY_API.Models;

namespace THMY_API.Services
{
    /// <summary>
    /// Service interface for validating if entities can be safely deleted from the database.
    /// Since entities don't have explicit foreign key relationships in the database,
    /// this service manually checks for dependencies before allowing deletion.
    /// </summary>
    public interface IDeletionValidator
    {
        /// <summary>
        /// Validates if a permission can be deleted by checking if it's referenced in RolePermission.
        /// </summary>
        /// <param name="permissionId">The ID of the permission to validate</param>
        /// <returns>Validation result indicating if deletion is allowed and why</returns>
        Task<DeletionValidationResult> CanDeletePermission(int permissionId);

        /// <summary>
        /// Validates if a role can be deleted by checking if it's referenced in EmpRole or RolePermission.
        /// </summary>
        /// <param name="roleId">The ID of the role to validate</param>
        /// <returns>Validation result indicating if deletion is allowed and why</returns>
        Task<DeletionValidationResult> CanDeleteRole(int roleId);
    }

    /// <summary>
    /// Result of a deletion validation check.
    /// </summary>
    public class DeletionValidationResult
    {
        /// <summary>
        /// Indicates whether the entity can be safely deleted.
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Detailed reason explaining why deletion was denied (if CanDelete is false).
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// List of specific dependencies that prevent deletion.
        /// For example: ["5 Role Permission assignments", "2 Employee Role assignments"]
        /// </summary>
        public List<string> Dependencies { get; set; } = new();

        /// <summary>
        /// The actual dependent objects that are preventing deletion.
        /// </summary>
        public DependentData? DependentData { get; set; }
    }
}

