using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Access.Repositories;
using System.Data;

namespace Access.Services.Repo
{
    public class RoleService
    {
        private readonly RoleRepository _roleRepository;

        public RoleService(string connectionString)
        {
            _roleRepository = new RoleRepository(connectionString);
        }

        public async Task InsertRoleAsync(string id, string name, string normalizedName, string concurrencyStamp)
        {
            await _roleRepository.InsertRoleAsync(id, name, normalizedName, concurrencyStamp);
        }

        public async Task<DataTable> GetRolesAsync()
        {
            return await _roleRepository.GetRolesAsync();
        }
    }
}
