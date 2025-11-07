namespace THMY_API.Models
{

    public class Permission
    {
        public required int permissionId { get; set; }
        public required string permissionName { get; set; }
        public string? permissionDescription { get; set; }
        public required int systemId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.Now;
        public DateTime updatedAt { get; set; } = DateTime.Now;
    }
}

