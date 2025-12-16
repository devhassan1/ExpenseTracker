
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
            var response = new LoginResponseModel { IsValid = false };

            var user = await _repo.GetByName(username);

            // 1. User not found
            if (user == null)
                return response;

            // 2. Password mismatch (TEMP – replace with hash check later)
            if (user.PasswordHash != password)
                return (response);

            // 3. Valid login
            response.UserID = (int)user.Id;
            response.Role = await _repo.GetRoleByID(user.RoleId);
            response.IsValid = true;

            return response;
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
        public Task<Result<long>> RegisterUser(RegisterRequest req, CancellationToken ct)
            => _repo.RegisterUser(req, ct);

        private sealed record Entry(string Password, long UserId, string[] Roles);
        private sealed record UserConfig(string Username, string Password, long UserId, string[]? Roles);
    }
}