using FStreak.Application.Services;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using FStreak.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FStreak.Application.Services.Interface;
using FStreak.Application.Services.Implementation;

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
            services.AddScoped<IStreakService, StreakService>();

            return services;
        }
    }
}