using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AllSub.WebMVC.Data;
using Polly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AllSub.WebMVC
{
    public class SeedData
    {
        public static async Task EnsureSeedData(IServiceScope scope, IConfiguration configuration, Microsoft.Extensions.Logging.ILogger logger)
        {
            var retryPolicy = CreateRetryPolicy(configuration, logger);
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await retryPolicy.ExecuteAsync(async () =>
            {
                await context.Database.MigrateAsync();

                var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var adminUser = await userMgr.FindByNameAsync("admin@email.com");

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = "admin@email.com",
                        Email = "admin@email.com",
                        EmailConfirmed = true,
                        Id = Guid.NewGuid().ToString(),
                        PhoneNumber = "1234567890",
                    };

                    var result = userMgr.CreateAsync(adminUser, "Pass123$admin").Result;

                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    logger.LogDebug("adminUser created");
                }
                else
                {
                    logger.LogDebug("adminUser already exists");
                }
            });
        }

        private static AsyncPolicy CreateRetryPolicy(IConfiguration configuration, Microsoft.Extensions.Logging.ILogger logger)
        {
            var retryMigrations = false;
            bool.TryParse(configuration["RetryMigrations"], out retryMigrations);

            // Only use a retry policy if configured to do so.
            // When running in an orchestrator/K8s, it will take care of restarting failed services.
            if (retryMigrations)
            {
                return Policy.Handle<Exception>().
                    WaitAndRetryForeverAsync(
                        sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                        onRetry: (exception, retry, timeSpan) =>
                        {
                            logger.LogWarning(
                                exception,
                                "Exception {ExceptionType} with message {Message} detected during database migration (retry attempt {retry})",
                                exception.GetType().Name,
                                exception.Message,
                                retry);
                        }
                    );
            }

            return Policy.NoOpAsync();
        }
    }
}
