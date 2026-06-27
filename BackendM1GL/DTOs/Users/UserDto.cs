namespace BackendM1GL.DTOs.Users
{
    public record UserDto(
    Guid Id, string FirstName, string LastName, string Email,
    string Role, bool IsActive, string? AvatarUrl, string? Phone,
    string? Department, DateTime CreatedAt);
}
