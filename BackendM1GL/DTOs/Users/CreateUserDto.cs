namespace BackendM1GL.DTOs.Users
{
    public record CreateUserDto(
    string FirstName, string LastName, string Email, string Password,
    string Role, string? Phone, string? Department);
}
