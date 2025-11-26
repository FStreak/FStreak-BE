using FStreak.Application.Services;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using FStreak.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FStreak.Application.Services.Interface;
using FStreak.Application.Services.Implementation;
using WebPush;
using Microsoft.Extensions.Caching.Distributed;

namespace FStreak.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFStreakServices(this IServiceCollection services, string connectionString)
        {
            // Add DbContext
            services.AddDbContext<FStreakDbContext>(options =>
                options
                //.UseSqlServer(connectionString));
                .UseMySql( 
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    mysqlOptions => mysqlOptions.EnableRetryOnFailure()
                )
            );

            // Add Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add Generic Repository (will be instantiated through UnitOfWork)
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Add Services
            services.AddScoped<IStreakRealtimeNotifier, FStreak.API.Services.StreakSignalRNotifier>();
            services.AddScoped<IStreakService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<StreakService>>();
                var unitOfWork = sp.GetRequiredService<IUnitOfWork>();
                var configuration = sp.GetRequiredService<IConfiguration>();
                var cache = sp.GetRequiredService<IDistributedCache>();
                var achievementService = sp.GetRequiredService<IAchievementService>();
                var notifier = sp.GetService<IStreakRealtimeNotifier>();
                return new StreakService(logger, unitOfWork, configuration, cache, achievementService, notifier);
            });
            services.AddScoped<IReminderService, ReminderService>();
            services.AddScoped<IReminderRepository, ReminderRepository>();

            // Add Push Notification Services
            services.AddScoped<IPushSubscriptionRepository, PushSubscriptionRepository>();
            services.AddScoped<IPushNotificationService, PushNotificationService>();
            services.AddSingleton<VapidDetails>(sp => new VapidDetails(
                "mailto:support@fstreak.com",  // Replace with your contact email
                "BNxviC3KknO_L3GZfXJQ2F5P0XPgAB0lGfX22w8O3MYZzXC-4G_Dy3MbTiQFKnzVQq-JLYD5YMD4peGIlSK4mYM",
                "eO5Tgc4dQW3CXv99pjbL91q5CaxBhxe_Z-xjD1CqshI"
            ));
            services.AddSingleton<WebPushClient>();

            return services;
        }
    }
}
    