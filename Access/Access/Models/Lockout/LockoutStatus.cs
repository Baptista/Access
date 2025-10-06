namespace Access.Models.Lockout
{
    public class LockoutStatus
    {
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int RemainingMinutes { get; set; }
    }
}
