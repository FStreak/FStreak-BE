using Microsoft.EntityFrameworkCore;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using FStreak.Infrastructure.Repositories;
using FStreak.Application.Services;

namespace FStreak.API
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFStreakServices(this IServiceCollection services, string connectionString)
        {
            // Add DbContext
            services.AddDbContext<FStreakDbContext>(options =>
                options.UseSqlServer(connectionString));

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