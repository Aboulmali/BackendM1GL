namespace BackendM1GL.DTOs.Users
{
    public record UpdateUserDto(
     string FirstName, string LastName, string? Phone,
     string? Department, string? AvatarUrl);
}
