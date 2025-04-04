using System;
using System.Data;
using System.Threading.Tasks;
using Access.Repositories;

namespace Access.Services.Repo
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(string connectionString)
        {
            _userRepository = new UserRepository(connectionString);
        }

        public async Task InsertUserAsync(
            string id,
            string userName,
            string normalizedUserName,
            string email,
            string normalizedEmail,
            bool emailConfirmed,
            string passwordHash,
            string securityStamp,
            string concurrencyStamp,
            string phoneNumber,
            bool phoneNumberConfirmed,
            bool twoFactorEnabled,
            DateTime? lockoutEnd,
            bool lockoutEnabled,
            int accessFailedCount)
        {
            await _userRepository.InsertUserAsync(
                id, userName, normalizedUserName, email, normalizedEmail, emailConfirmed,
                passwordHash, securityStamp, concurrencyStamp, phoneNumber, phoneNumberConfirmed,
                twoFactorEnabled, lockoutEnd, lockoutEnabled, accessFailedCount);
        }

        public async Task<DataTable> GetUsersAsync()
        {
            return await _userRepository.GetUsersAsync();
        }
    }
}
