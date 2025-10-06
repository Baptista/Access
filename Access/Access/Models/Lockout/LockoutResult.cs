namespace Access.Models.Lockout
{
    public class LockoutResult
    {
        public int FailedCount { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }
}
