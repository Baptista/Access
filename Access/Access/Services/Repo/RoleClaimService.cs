using Access.Repositories;
using System.Data;

namespace Access.Services.Repo
{
    public class RoleClaimService
    {
        private readonly RoleClaimRepository _roleClaimRepository;

        public RoleClaimService(string connectionString)
        {
            _roleClaimRepository = new RoleClaimRepository(connectionString);
        }

        public async Task InsertRoleClaimAsync(string roleId, string claimType, string claimValue)
        {
            await _roleClaimRepository.InsertRoleClaimAsync(roleId, claimType, claimValue);
        }

        public async Task<DataTable> GetRoleClaimsAsync()
        {
            return await _roleClaimRepository.GetRoleClaimsAsync();
        }
    }
}
