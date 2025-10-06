// File: Access/Services/Background/LockoutCleanupService.cs
using Access.DataAccess;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Access.Services.Background
{
    public class LockoutCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<LockoutCleanupService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(5); // Run every 5 minutes

        public LockoutCleanupService(
            IServiceProvider serviceProvider,
            ILogger<LockoutCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Lockout Cleanup Service is starting.");

            using PeriodicTimer timer = new PeriodicTimer(_period);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                    await CleanupExpiredLockouts();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Lockout Cleanup Service is stopping.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired lockouts.");
                }
            }
        }

        private async Task CleanupExpiredLockouts()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var clearedCount = await userRepository.ClearExpiredLockoutsAsync();

                if (clearedCount > 0)
                {
                    _logger.LogInformation($"Cleared {clearedCount} expired lockout(s).");
                }
            }
        }
    }
}

// To register this service in Program.cs (optional):
// builder.Services.AddHostedService<LockoutCleanupService>();