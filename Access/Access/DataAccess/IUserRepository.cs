using Access.Data;
using Access.Models.Lockout;
using Microsoft.Data.SqlClient;

namespace Access.DataAccess
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByEmailAsync(
            string email,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<ApplicationUser> GetUserByUserNameAsync(
            string userName,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<ApplicationUser> GetUserByIdAsync(
            string id,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> CreateUserAsync(
            ApplicationUser user,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> UpdateUserAsync(
            ApplicationUser user,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> ConfirmEmailAsync(
            string userId,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> UpdatePasswordHashAsync(
            string userId,
            string passwordHash,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<int> IncrementAccessFailedCountAsync(
            string userId,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> ResetAccessFailedCountAsync(
            string userId,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> SetLockoutEndDateAsync(
            string userId,
            DateTimeOffset? lockoutEnd,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> CheckPasswordAsync(
            string userId,
            string passwordHash,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<List<string>> GetUserRolesAsync(
            string userId,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> IsUserInRoleAsync(
            string userId,
            string roleName,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> AddUserToRoleAsync(
            string userId,
            string roleName,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> RemoveUserFromRoleAsync(
            string userId,
            string roleName,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<string> GetAuthenticationTokenAsync(
            string userId,
            string loginProvider,
            string name,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> SetAuthenticationTokenAsync(
            string userId,
            string loginProvider,
            string name,
            string value,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> RemoveAuthenticationTokenAsync(
            string userId,
            string loginProvider,
            string name,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<LockoutResult> HandleFailedLoginAsync(
            string userId,
            int maxFailedAttempts = 3,
            int lockoutMinutes = 5,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<bool> HandleSuccessfulLoginAsync(
            string userId,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<LockoutStatus> CheckUserLockoutAsync(
            string userId,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<UserWithLockoutInfo> GetUserWithLockoutInfoAsync(
            string userName,
            SqlConnection connection = null,
            SqlTransaction transaction = null);

        Task<int> ClearExpiredLockoutsAsync(
            SqlConnection connection = null,
            SqlTransaction transaction = null);

    }
}
