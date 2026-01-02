using InternalSystem_ModelContext.Models.SQLite;

namespace THMY_API.Models
{
    /// <summary>
    /// Response model for deletion error (409 Conflict) from the backend API.
    /// Contains detailed information about why deletion was blocked.
    /// </summary>
    public class DeletionErrorResponse
    {
        public string error { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public List<string> dependencies { get; set; } = new();
        public int statusCode { get; set; }
        public DependentData? dependentData { get; set; }
    }

    /// <summary>
    /// Contains the actual objects that are preventing deletion.
    /// </summary>
    public class DependentData
    {
        /// <summary>
        /// List of employees assigned to this role (for Role deletion)
        /// </summary>
        public List<EmpRole>? employeeRoles { get; set; }

        /// <summary>
        /// List of permissions assigned to this role (for Role deletion)
        /// </summary>
        public List<RolePermission>? rolePermissions { get; set; }

        /// <summary>
        /// List of roles using this permission (for Permission deletion)
        /// </summary>
        public List<Role>? roles { get; set; }
    }

}

