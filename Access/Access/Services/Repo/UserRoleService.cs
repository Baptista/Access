using Access.Repositories;
using System.Data;

namespace Access.Services.Repo
{
    public class UserRoleService
    {
        private readonly UserRoleRepository _userRoleRepository;

        public UserRoleService(string connectionString)
        {
            _userRoleRepository = new UserRoleRepository(connectionString);
        }

        public async Task InsertUserRoleAsync(string userId, string roleId)
        {
            await _userRoleRepository.InsertUserRoleAsync(userId, roleId);
        }

        public async Task<DataTable> GetUserRolesAsync()
        {
            return await _userRoleRepository.GetUserRolesAsync();
        }
    }
}
