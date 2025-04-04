using Access.Repositories;
using System.Data;

namespace Access.Services.Repo
{
    public class UserClaimService
    {
        private readonly UserClaimRepository _userClaimRepository;

        public UserClaimService(string connectionString)
        {
            _userClaimRepository = new UserClaimRepository(connectionString);
        }

        public async Task InsertUserClaimAsync(string userId, string claimType, string claimValue)
        {
            await _userClaimRepository.InsertUserClaimAsync(userId, claimType, claimValue);
        }

        public async Task<DataTable> GetUserClaimsAsync()
        {
            return await _userClaimRepository.GetUserClaimsAsync();
        }
    }
}
