using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExecutionOS.API.Data;
using ExecutionOS.API.DTOs;
using ExecutionOS.API.Models;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ExecutionOS.API.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse> GoogleLogin(string idToken)
    {
        var googleClientId = _config["Auth:GoogleClientId"]
            ?? throw new InvalidOperationException("Google Client ID not configured.");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { googleClientId }
            });
        }
        catch (InvalidJwtException)
        {
            throw new InvalidOperationException("Invalid Google token.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject);

        if (user == null)
        {
            // Check if email already exists (user previously created without Google)
            user = await _db.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user != null)
            {
                // Link Google account to existing user
                user.GoogleId = payload.Subject;
                user.ProfilePicture = payload.Picture;
                user.Name = payload.Name ?? user.Name;
                user.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new user
                user = new User
                {
                    Email = payload.Email,
                    Name = payload.Name ?? payload.Email,
                    GoogleId = payload.Subject,
                    ProfilePicture = payload.Picture
                };
                _db.Users.Add(user);
            }

            await _db.SaveChangesAsync();
        }
        else
        {
            // Update profile info on each login
            user.ProfilePicture = payload.Picture;
            user.Name = payload.Name ?? user.Name;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        var token = GenerateJwt(user);

        return new AuthResponse(
            token,
            user.Id,
            user.Email,
            user.Name,
            user.ProfilePicture
        );
    }

    public async Task<AuthResponse?> GetMe(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return null;

        var token = GenerateJwt(user);
        return new AuthResponse(token, user.Id, user.Email, user.Name, user.ProfilePicture);
    }

    private string GenerateJwt(User user)
    {
        var secret = _config["Auth:JwtSecret"]
            ?? throw new InvalidOperationException("JWT secret not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Auth:JwtIssuer"] ?? "ExecutionOS",
            audience: _config["Auth:JwtAudience"] ?? "ExecutionOS",
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
