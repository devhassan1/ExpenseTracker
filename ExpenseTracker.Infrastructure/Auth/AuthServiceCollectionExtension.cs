using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Infrastructure.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration cfg)
    {
        // Dev login service
        services.AddScoped<IAuthService, DevAuthService>();

        var issuer = cfg["Jwt:Issuer"];
        var audience = cfg["Jwt:Audience"];
        var key = cfg["Jwt:Key"] ?? throw new Exception("Missing JWT Key");

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // set true in prod behind HTTPS
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
                    ValidateLifetime = true,
                    // Ensure ASP.NET knows which claim holds roles
                    RoleClaimType = ClaimTypes.Role,     // or "role" if you use that
                    NameClaimType = ClaimTypes.Name
                };
            });


        services.AddAuthorization(options =>
        {
            options.AddPolicy("CanAddExpense", p => p.RequireRole("User", "Admin", "SuperAdmin"));
            options.AddPolicy("CanViewOrgExpenses", p => p.RequireRole("Admin", "SuperAdmin"));
            options.AddPolicy("CanViewAll", p => p.RequireRole("SuperAdmin"));
        });

        return services;
    }
}
