using DotNetEnv;
using FStreak.Application.Services;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using FStreak.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

try
{
    if (File.Exists("../.env"))
    {
        Env.Load("../.env");
    }
}
catch { }

var connectionString = $"server={Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost"};" +
                      $"port={Environment.GetEnvironmentVariable("DB_PORT") ?? "3306"};" +
                      $"database={Environment.GetEnvironmentVariable("DB_NAME") ?? "FStreak"};" +
                      $"user={Environment.GetEnvironmentVariable("DB_USERNAME") ?? "root"};" +
                      $"password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "password"};";

// Ghi đè giá trị ConnectionString trong cấu hình
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<FStreakDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(9, 0, 0)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    ));

builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FStreak API", Version = "v1" });
});

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
//}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

