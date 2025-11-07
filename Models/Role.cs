namespace THMY_API.Models
{
    public class Role
    {
        public required int roleId { get; set; }
        public required string roleName { get; set; }
        public string? roleDescription { get; set; }
        public DateTime createdAt { get; set; } = DateTime.Now;
        public DateTime updatedAt { get; set; } = DateTime.Now;
    }
}

