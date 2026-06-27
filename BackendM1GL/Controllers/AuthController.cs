using BackendM1GL.Data;
using BackendM1GL.DTOs;
using BackendM1GL.DTOs.Auth;
using BackendM1GL.DTOs.Users;
using BackendM1GL.Entities;
using BackendM1GL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendM1GL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AppDbContext db,
    ITokenService tokenSvc,
    ILogger<AuthController> log) : ControllerBase
{
    // ── REGISTER ─────────────────────────────────────────────────────
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email.ToLowerInvariant()))
            return Conflict(new { message = "Email déjà utilisé" });

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        log.LogInformation("Nouvel utilisateur inscrit : {Email}", user.Email);
        return Ok(await BuildAuthResponse(user));
    }

    // ── LOGIN ────────────────────────────────────────────────────────
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLowerInvariant());

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            log.LogWarning("Tentative échouée : {Email}", dto.Email);
            return Unauthorized(new { message = "Email ou mot de passe incorrect" });
        }

        if (!user.IsActive)
            return Forbid();

        log.LogInformation("Connexion réussie : {Email}", user.Email);
        return Ok(await BuildAuthResponse(user));
    }

    // ── REFRESH TOKEN (Approche 2) ───────────────────────────────────
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenDto dto)
    {
        // ✅ Cherche dans la table RefreshTokens (pas Users)
        var refreshToken = await db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt =>
                rt.Token == dto.RefreshToken &&
                rt.RevokedAt == null &&
                rt.ExpiresAt > DateTime.UtcNow);

        if (refreshToken is null)
            return Unauthorized(new { message = "Refresh token invalide ou expiré" });

        // ✅ Révoque l'ancien token (rotation)
        refreshToken.RevokedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // ✅ Génère de nouveaux tokens
        return Ok(await BuildAuthResponse(refreshToken.User));
    }

    // ── LOGOUT (Approche 2) ──────────────────────────────────────────
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenDto dto)
    {
        // ✅ Cherche dans la table RefreshTokens
        var refreshToken = await db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

        if (refreshToken is not null && refreshToken.RevokedAt == null)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return NoContent();
    }

    // ── HELPER : Construit la réponse d'authentification ─────────────
    private async Task<AuthResponseDto> BuildAuthResponse(User user)
    {
        var accessToken = tokenSvc.GenerateAccessToken(user);
        var refreshTokenValue = tokenSvc.GenerateRefreshToken();

        // ✅ Crée une NOUVELLE entrée dans RefreshTokens (pas écraser)
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = GetClientIp(),
            DeviceInfo = Request.Headers.UserAgent.ToString()
        };

        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        return new AuthResponseDto(accessToken, refreshTokenValue, ToUserDto(user));
    }

    // ── HELPER : IP du client ────────────────────────────────────────
    private string? GetClientIp() =>
        HttpContext.Connection.RemoteIpAddress?.ToString();

    // ── HELPER : Mapping User → UserDto ──────────────────────────────
    private static UserDto ToUserDto(User u) =>
        new(u.Id, u.FirstName, u.LastName, u.Email, u.Role,
            u.IsActive, u.AvatarUrl, u.Phone, u.Department, u.CreatedAt);
}