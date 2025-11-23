using DotNetEnv;
using FStreak.API.Hubs;
using FStreak.Application.Configuration;
using FStreak.Application.Services.Implementation;
using FStreak.Application.Services.Interface;
using FStreak.Domain.Entities;
using FStreak.Domain.Interfaces;
using FStreak.Infrastructure.Data;
using FStreak.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// 1. CONFIGURATION
// -----------------------------

// Load environment variables
try
{
    if (File.Exists("../.env"))
    {
        Env.Load("../.env");
    }
}
catch { }

// Configure connection string
var connectionString = $"server={Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost"};" +
                      $"port={Environment.GetEnvironmentVariable("DB_PORT") ?? "3306"};" +
                      $"database={Environment.GetEnvironmentVariable("DB_NAME") ?? "FStreak"};" +
                      $"user={Environment.GetEnvironmentVariable("DB_USERNAME") ?? "root"};" +
                      $"password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "password"};";

builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

// -----------------------------
// 2. CORE SERVICES
// -----------------------------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add DbContext
builder.Services.AddDbContext<FStreakDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    ));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<FStreakDbContext>()
.AddDefaultTokenProviders();

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"])
        )
    };
});

// -----------------------------
// 3. INFRASTRUCTURE SERVICES
// -----------------------------

// Add CORS (allow all origins for cross-browser compatibility)
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;

});

// Add Caching
if (!string.IsNullOrEmpty(builder.Configuration.GetConnectionString("Redis")))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
        options.InstanceName = "FStreak_";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// -----------------------------
// 4. APPLICATION SERVICES
// -----------------------------

// Configure Cloudinary Settings
builder.Services.Configure<CloudinarySettings>(options =>
{
    options.CloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")
        ?? builder.Configuration["Cloudinary:CloudName"] ?? "";
    options.ApiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")
        ?? builder.Configuration["Cloudinary:ApiKey"] ?? "";
    options.ApiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
        ?? builder.Configuration["Cloudinary:ApiSecret"] ?? "";
});

// Add Unit of Work and Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add Domain Services
builder.Services.AddScoped<IAgoraService, AgoraService>();
builder.Services.AddScoped<IStudyRoomService, StudyRoomService>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IStudyRoomService, StudyRoomService>();
builder.Services.AddScoped<IFriendService, FriendService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IShopService, ShopService>();


// -----------------------------
// 5. API DOCS & MONITORING
// -----------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "F-Streak API", Version = "v1" });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        //Type = SecuritySchemeType.ApiKey,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// -----------------------------
// 6. APPLICATION PIPELINE
// -----------------------------

var app = builder.Build();

// Configure development environment
    app.UseSwagger();
    app.UseRouting();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "F-Streak API V1");
        c.RoutePrefix = string.Empty;
    });

app.UseCors("DefaultCorsPolicy");

// Configure middleware pipeline
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure endpoints

app.MapControllers();
app.MapHub<StudyRoomHub>("/hubs/studyroom");
app.MapHub<FStreak.API.Hubs.StreakHub>("/hubs/streak");

// -----------------------------
// 7. INITIALIZATION
// -----------------------------

// Initialize roles
try
{
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var roles = new[] { "Admin", "Moderator", "User", "Teacher" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error initializing roles: {ex.Message}");
}

// Start the app
app.Run();
