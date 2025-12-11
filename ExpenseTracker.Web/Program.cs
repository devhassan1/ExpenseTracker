//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();

//using ExpenseTracker.Application.DTOs;
//using ExpenseTracker.Application.Interfaces;
//using ExpenseTracker.Application.UseCases.Expenses;
//using ExpenseTracker.Application.UseCases.Reports;
//using ExpenseTracker.Infrastructure;
//using ExpenseTracker.Infrastructure.Auth;
//using ExpenseTracker.Infrastructure.Exports;
//using ExpenseTracker.Infrastructure.Persistance;
//using ExpenseTracker.Infrastructure.Repositories;
//using ExpenseTracker.Web.Facades;

//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;

//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//var builder = WebApplication.CreateBuilder(args);

//// ------------------------------
//// Connection string
//// ------------------------------
//var conn = builder.Configuration.GetConnectionString("DefaultConnection")
//    ?? "User Id=system;Password=hassan@tps;Data Source=localhost:1521/XEPDB1";

//// ------------------------------
//// DbContext
//// ------------------------------
//builder.Services.AddDbContext<OracleDbContext>(options => options.UseOracle(conn));

//// ------------------------------
//// Repositories
//// ------------------------------
//builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
//builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
//builder.Services.AddScoped<ITagRepository, TagRepository>();

//// ------------------------------
//// Cross-cutting
//// ------------------------------
//builder.Services.AddSingleton<IClock, SystemClock>();
//builder.Services.AddScoped<IExportService, CsvExportService>(); // replace with format-aware factory later

//// HttpContext accessor + CurrentUser from HttpContext.User
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<ICurrentUser>(sp =>
//{
//    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
//    return new CurrentUser(httpContext?.User ?? new ClaimsPrincipal());
//});

//// ------------------------------
//// Use cases
//// ------------------------------
//builder.Services.AddScoped<AddExpenseUseCase>();
//builder.Services.AddScoped<ListExpensesUseCase>();
//builder.Services.AddScoped<ExpenseTracker.Application.UseCases.Reports.ExportExpensesUseCase>();

//// ------------------------------
//// Facades
//// ------------------------------
//builder.Services.AddScoped<ExpenseFacade>();
//builder.Services.AddScoped<ReportFacade>();

//// ------------------------------
//// Auth (JWT) — Validation
//// ------------------------------
//builder.Services
//    .AddAuthentication(options =>
//    {
//        // Set Bearer as default auth scheme
//        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    })
//    .AddJwtBearer(options =>
//    {
//        var issuer = builder.Configuration["Jwt:Issuer"];
//        var audience = builder.Configuration["Jwt:Audience"];
//        var key = builder.Configuration["Jwt:Key"];

//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateIssuerSigningKey = true,
//            ValidateLifetime = true,

//            ValidIssuer = issuer,
//            ValidAudience = audience,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? string.Empty)),
//            // If your tokens store roles under a custom claim (e.g., "roles"), uncomment:
//            // RoleClaimType = "roles"
//        };

//        // Optional for local dev without HTTPS:
//        // options.RequireHttpsMetadata = false;
//    });

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("CanAddExpense", p => p.RequireRole("User", "Admin", "SuperAdmin"));
//    options.AddPolicy("CanViewOrgExpenses", p => p.RequireRole("Admin", "SuperAdmin"));
//    options.AddPolicy("CanViewAll", p => p.RequireRole("SuperAdmin"));
//});

//// ------------------------------
//// Simple Auth Service for login (TEMP)
//// Reads users from configuration: Auth:Users
//// Replace with your own repo/Identity later.
//// ------------------------------
//builder.Services.AddSingleton<IAuthService, DevAuthService>();

//var app = builder.Build();

//app.UseAuthentication();
//app.UseAuthorization();

//// ------------------------------
//// Endpoints: Expenses & Reports
//// ------------------------------
//app.MapPost("/api/expenses", async (ExpenseFacade facade, AddExpenseRequest req, CancellationToken ct) =>
//{
//    var result = await facade.AddAsync(req, ct);
//    return result.IsSuccess ? Results.Ok(new { id = result.Value }) : Results.BadRequest(result.Error);
//}).RequireAuthorization("CanAddExpense");

//app.MapGet("/api/expenses", async (ExpenseFacade facade, DateTime from, DateTime to, long? forUserId, CancellationToken ct) =>
//{
//    var result = await facade.ListAsync(new(from, to, forUserId), ct);
//    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
//}).RequireAuthorization("CanAddExpense"); // adjust for org-level with "CanViewOrgExpenses"

//app.MapGet("/api/reports/export", async (ReportFacade facade, DateTime from, DateTime to, long? forUserId, string format, CancellationToken ct) =>
//{
//    var result = await facade.ExportAsync(new(from, to, forUserId, format), ct);
//    if (!result.IsSuccess) return Results.BadRequest(result.Error);

//    var fmt = format.ToLowerInvariant();
//    var contentType = fmt switch
//    {
//        "csv" => "text/csv",
//        "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//        "pdf" => "application/pdf",
//        _ => "application/octet-stream"
//    };

//    var ext = fmt == "csv" ? "csv" : fmt == "excel" ? "xlsx" : "pdf";
//    var fileName = $"expenses_{from:yyyyMMdd}_{to:yyyyMMdd}.{ext}";

//    return Results.File(result.Value!, contentType, fileName);
//}).RequireAuthorization("CanAddExpense");

//// ------------------------------
//// Login endpoint — issues JWT
//// ------------------------------
//app.MapPost("/api/auth/login", async (
//    IAuthService auth,
//    LoginRequest req,
//    IConfiguration cfg,
//    CancellationToken ct) =>
//{
//    var authResult = await auth.ValidateCredentialsAsync(req.Username, req.Password, ct);
//    if (!authResult.IsValid)
//        return Results.Unauthorized();

//    var issuer = cfg["Jwt:Issuer"];
//    var audience = cfg["Jwt:Audience"];
//    var key = cfg["Jwt:Key"];

//    if (string.IsNullOrWhiteSpace(issuer) ||
//        string.IsNullOrWhiteSpace(audience) ||
//        string.IsNullOrWhiteSpace(key))
//    {
//        return Results.Problem("JWT not configured (Issuer/Audience/Key).");
//    }

//    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
//    var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

//    var claims = new List<Claim>
//    {
//        new(ClaimTypes.NameIdentifier, authResult.UserId.ToString()),
//        new(ClaimTypes.Name, req.Username),
//    };
//    // Add roles to claims (used by policy RequireRole)
//    claims.AddRange(authResult.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

//    var token = new JwtSecurityToken(
//        issuer: issuer,
//        audience: audience,
//        claims: claims,
//        notBefore: DateTime.UtcNow,
//        expires: DateTime.UtcNow.AddHours(8), // adjust lifetime as needed
//        signingCredentials: creds
//    );

//    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

//    return Results.Ok(new
//    {
//        token = jwt,
//        token_type = "Bearer",
//        expires_in = (int)TimeSpan.FromHours(8).TotalSeconds
//    });
//});

//app.MapGet("/", () => "Expense Tracker API running (.NET 8 + Oracle + EF Core)");

//app.UseDefaultFiles(); // serves index.html by default
//app.UseStaticFiles();

//app.Run();


//// ==============================
//// Types & TEMP auth service
//// ==============================

//public record LoginRequest(string Username, string Password);

//public interface IAuthService
//{
//    Task<(bool IsValid, long UserId, string[] Roles)> ValidateCredentialsAsync(
//        string username, string password, CancellationToken ct);
//}

///// <summary>
///// TEMP dev auth service that reads users from configuration.
///// Expected config:
///// "Auth:Users": [ { "Username": "...", "Password": "...", "UserId": 1, "Roles": ["User","Admin"] } ]
///// NOTE: Do NOT use plain passwords in production. Replace with your own store + hashing.
///// </summary>
//public sealed class DevAuthService : IAuthService
//{
//    private readonly Dictionary<string, Entry> _users;

//    public DevAuthService(IConfiguration configuration)
//    {
//        // Read users from config; if absent, use empty.
//        var section = configuration.GetSection("Auth:Users").Get<List<UserConfig>>() ?? new();
//        _users = section.ToDictionary(
//            u => u.Username,
//            u => new Entry(u.Password, u.UserId, u.Roles ?? Array.Empty<string>()),
//            StringComparer.OrdinalIgnoreCase);
//    }

//    public Task<(bool IsValid, long UserId, string[] Roles)> ValidateCredentialsAsync(
//        string username, string password, CancellationToken ct)
//    {
//        if (_users.TryGetValue(username, out var entry) && entry.Password == password)
//        {
//            return Task.FromResult((true, entry.UserId, entry.Roles));
//        }
//        return Task.FromResult((false, 0L, Array.Empty<string>()));
//    }

//    private sealed record Entry(string Password, long UserId, string[] Roles);
//    private sealed record UserConfig(string Username, string Password, long UserId, string[]? Roles)
//    {
//    }
//    };



using ExpenseTracker.Application;
using ExpenseTracker.Infrastructure;
using ExpenseTracker.Infrastructure.Auth;
using ExpenseTracker.Infrastructure.Persistence;
using ExpenseTracker.Web;


var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPersistence(builder.Configuration)
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddFacades()
    .AddAuth(builder.Configuration);

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // MVC routing

app.MapGet("/", () => "Expense Tracker API running with Controllers");

app.Run();
