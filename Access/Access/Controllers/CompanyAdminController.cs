using Access.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Access.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "CompanyAdmin")]
    public class CompanyAdminController : ControllerBase
    {

        private readonly IUserRepository _userRepository;

        public CompanyAdminController(IUserRepository userRepository)
        {
            _userRepository = userRepository;

        }

        [HttpGet("users")]
        public async Task<IActionResult> GetCompanyUsers()
        {
            throw new NotImplementedException();
        }

        [HttpPost("deactivate-user")]
        public async Task<IActionResult> DeactivateUser(string email)
        {
            throw new NotImplementedException();
        }

        [HttpGet("export-data")]
        public async Task<IActionResult> ExportCompanyData()
        {
            throw new NotImplementedException();
        }

        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }
    }
}
