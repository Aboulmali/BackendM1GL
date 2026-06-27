using BackendM1GL.DTOs.Users;

namespace BackendM1GL.DTOs.Auth
{
    public record AuthResponseDto(string AccessToken, string RefreshToken, UserDto
User);
}
