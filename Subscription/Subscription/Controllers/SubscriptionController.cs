using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subscription.Data;

namespace Subscription.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionContext _context;

        public SubscriptionController(SubscriptionContext context)
        {
            _context = context;
        }

        [HttpGet("migrate")]
        public IActionResult ApplyMigrations()
        {
            try
            {
                _context.Database.Migrate();
                return Ok("Migrations applied successfully.");
            }
            catch (Exception ex)
            {                
                return Ok("Error migrate");
            }
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckSubscription([FromQuery] string domain)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Domain == domain);

            if (subscription == null || subscription.ValidUntil < DateTime.Now)
            {
                return NotFound("No active subscription for this domain.");
            }

            return Ok("Subscription is active.");
        }
    }
}
