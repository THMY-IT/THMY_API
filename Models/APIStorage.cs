namespace THMY_API.Models
{
    public class APIStorage
    {
        public int id { get; set; }
        public required string applicationName { get; set; }
        public required string apiSecret { get; set; }
        public DateTime createdAt { get; set; } = DateTime.Now;
        public DateTime updatedAt { get; set; } = DateTime.Now;
    }

}
