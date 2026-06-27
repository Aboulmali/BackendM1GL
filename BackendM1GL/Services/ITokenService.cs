using BackendM1GL.Entities;

namespace BackendM1GL.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
    }
}
