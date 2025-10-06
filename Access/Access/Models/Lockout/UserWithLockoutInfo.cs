using Access.Data;

namespace Access.Models.Lockout
{
    public class UserWithLockoutInfo : ApplicationUser
    {
        public bool IsCurrentlyLockedOut { get; set; }
        public int LockoutRemainingMinutes { get; set; }
    }
}
