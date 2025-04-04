using Access.Repositories;
using System.Data;

namespace Access.Services.Repo
{
    public class UserLoginService
    {
        private readonly UserLoginRepository _userLoginRepository;

        public UserLoginService(string connectionString)
        {
            _userLoginRepository = new UserLoginRepository(connectionString);
        }

        public async Task InsertUserLoginAsync(string loginProvider, string providerKey, string providerDisplayName, string userId)
        {
            await _userLoginRepository.InsertUserLoginAsync(loginProvider, providerKey, providerDisplayName, userId);
        }

        public async Task<DataTable> GetUserLoginsAsync()
        {
            return await _userLoginRepository.GetUserLoginsAsync();
        }
    }
}
