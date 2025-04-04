using Access.Repositories;
using System.Data;

namespace Access.Services.Repo
{
    public class UserTokenService
    {
        private readonly UserTokenRepository _userTokenRepository;

        public UserTokenService(string connectionString)
        {
            _userTokenRepository = new UserTokenRepository(connectionString);
        }

        public async Task InsertUserTokenAsync(string userId, string loginProvider, string name, string value)
        {
            await _userTokenRepository.InsertUserTokenAsync(userId, loginProvider, name, value);
        }

        public async Task<DataTable> GetUserTokensAsync()
        {
            return await _userTokenRepository.GetUserTokensAsync();
        }
    }
}
