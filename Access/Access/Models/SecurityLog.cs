namespace Access.Models
{
    public class SecurityLog
    {
        public int Id { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
