using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

namespace ExpenseTracker.Infrastructure.Auth;

public sealed class DevAuthService : IAuthService
{
    private readonly Dictionary<string, Entry> _users;

    public DevAuthService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Auth:Users").Get<List<UserConfig>>() ?? new();
        _users = section.ToDictionary(
            u => u.Username,
            u => new Entry(u.Password, u.UserId, u.Roles ?? Array.Empty<string>()),
            StringComparer.OrdinalIgnoreCase);
    }

    public Task<(bool IsValid, long UserId, string[] Roles)> ValidateCredentialsAsync(
        string username, string password, CancellationToken ct)
    {
        if (_users.TryGetValue(username, out var entry) && entry.Password == password)
            return Task.FromResult((true, entry.UserId, entry.Roles));

        return Task.FromResult((false, 0L, Array.Empty<string>()));
    }

    private sealed record Entry(string Password, long UserId, string[] Roles);
    private sealed record UserConfig(string Username, string Password, long UserId, string[]? Roles);
}

