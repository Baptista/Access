using Access.Data;
using Access.Models.Lockout;

namespace Access.DataAccess
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<ApplicationUser> GetUserByUserNameAsync(string userName);
        Task<ApplicationUser> GetUserByIdAsync(string id);
        Task<bool> CreateUserAsync(ApplicationUser user);
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<bool> ConfirmEmailAsync(string userId);
        Task<bool> UpdatePasswordHashAsync(string userId, string passwordHash);
        Task<int> IncrementAccessFailedCountAsync(string userId);
        Task<bool> ResetAccessFailedCountAsync(string userId);
        Task<bool> SetLockoutEndDateAsync(string userId, DateTimeOffset? lockoutEnd);
        Task<bool> CheckPasswordAsync(string userId, string passwordHash);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<bool> IsUserInRoleAsync(string userId, string roleName);
        Task<bool> AddUserToRoleAsync(string userId, string roleName);
        Task<bool> RemoveUserFromRoleAsync(string userId, string roleName);
        Task<string> GetAuthenticationTokenAsync(string userId, string loginProvider, string name);
        Task<bool> SetAuthenticationTokenAsync(string userId, string loginProvider, string name, string value);
        Task<bool> RemoveAuthenticationTokenAsync(string userId, string loginProvider, string name);
        Task<LockoutResult> HandleFailedLoginAsync(string userId, int maxFailedAttempts = 3, int lockoutMinutes = 5);
        Task<bool> HandleSuccessfulLoginAsync(string userId);
        Task<LockoutStatus> CheckUserLockoutAsync(string userId);
        Task<UserWithLockoutInfo> GetUserWithLockoutInfoAsync(string userName);
        Task<int> ClearExpiredLockoutsAsync();

    }
}
