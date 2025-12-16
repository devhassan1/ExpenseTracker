
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Common.Results;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpenseTracker.Infrastructure.Auth
{
    public sealed class DevAuthService : IAuthService
    {
        private readonly Dictionary<string, Entry> _users;
        private readonly IUserRepository _repo;
        private readonly IConfiguration _config;

        public DevAuthService(IConfiguration configuration, IUserRepository repo)
        {
            _repo = repo;
            _config = configuration;
            var section = configuration.GetSection("Auth:Users").Get<List<UserConfig>>() ?? new();
            _users = section.ToDictionary(
                u => u.Username,
                u => new Entry(u.Password, u.UserId, u.Roles ?? Array.Empty<string>()),
                StringComparer.OrdinalIgnoreCase);
        }

        public async Task<LoginResponseModel> ValidateCredentialsAsync(
            string username, string password)
        {
            LoginResponseModel loginResponseModel = new LoginResponseModel() { IsValid = false};
            var users = await _repo.GetByName(username);
            if (users != null || users.PasswordHash == password)
            {

                loginResponseModel.Role = await _repo.GetRoleByID(users.RoleId);
                loginResponseModel.UserID = (int)users.Id;
                loginResponseModel.IsValid = true;
            }
            return loginResponseModel;
        }



        public async Task<string> CreateToken(int userId, string username, string? role = null)
        {
            var jwt = _config.GetSection("Jwt");
            var issuer = jwt["Issuer"];
            var audience = jwt["Audience"];
            var key = jwt["Key"];
            var expiresMinutes = int.TryParse(jwt["ExpiresMinutes"], out var m) ? m : 60;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Name, username),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        // Delegate registration to repository (no _db here)
        public Task<Result<long>> RegisterAsync(RegisterRequest req, CancellationToken ct)
            => _repo.RegisterAsync(req, ct);

        private sealed record Entry(string Password, long UserId, string[] Roles);
        private sealed record UserConfig(string Username, string Password, long UserId, string[]? Roles);
    }
}