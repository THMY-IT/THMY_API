namespace THMY_API.Models
{
    public class EmpRole
    {
        public required string empId { get; set; }
        public required int roleId { get; set; }
        public required int systemId { get; set; }
        public DateTime createdAt { get; set; } = DateTime.Now;
        public DateTime updatedAt { get; set; } = DateTime.Now;
    }
}


