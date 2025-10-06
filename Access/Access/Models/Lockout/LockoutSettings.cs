namespace Access.Models.Lockout
{
    public class LockoutSettings
    {
        public int MaxFailedAccessAttempts { get; set; } = 3;
        public int DefaultLockoutTimeSpanInMinutes { get; set; } = 5;
        public bool AllowedForNewUsers { get; set; } = true;
    }
}
