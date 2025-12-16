namespace ExpenseTracker.Application.DTOs
{
    public sealed record UserDto(long Id, string Name, string? Email, string RoleName);
}
