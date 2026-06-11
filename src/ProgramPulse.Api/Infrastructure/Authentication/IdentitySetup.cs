using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Infrastructure.Persistence;

namespace ProgramPulse.Api.Infrastructure.Authentication;

/// <summary>
/// Registers ASP.NET Core Identity, JWT bearer authentication (Authorization header
/// first, HttpOnly cookie fallback), the named authorization policies, and the token
/// and cookie services. Replaces the prior <c>AddIdentityCore</c> registration so that
/// <see cref="SignInManager{TUser}"/> is available for the login flow.
/// </summary>
public static class IdentitySetup
{
    public static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Configure Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Bind auth cookie options and register the cookie service.
        services.Configure<AuthCookieOptions>(
            configuration.GetSection(AuthCookieOptions.SectionName));
        services.AddScoped<IAuthCookieService, AuthCookieService>();

        // Configure JWT
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Plaintext metadata exchange is permitted only in local dev; HTTPS is required elsewhere.
            options.RequireHttpsMetadata = !environment.IsDevelopmentOrLocal();
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // Read JWT from Authorization header first, then fall back to HttpOnly cookie
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Check Authorization header first
                    var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Token = authHeader["Bearer ".Length..].Trim();
                        return Task.CompletedTask;
                    }

                    // Fall back to HttpOnly cookie
                    var cookieOptions = context.HttpContext.RequestServices
                        .GetService<IOptions<AuthCookieOptions>>();
                    var cookieName = cookieOptions?.Value.AccessTokenCookieName ?? "access_token";
                    context.Token = context.Request.Cookies[cookieName];

                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorizationPolicies();

        // Register token service
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
