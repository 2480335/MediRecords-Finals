using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MediRecords.Domain.Entities;
using MediRecords.Dto.UserDtos;
using MediRecords.Repositories;
using BCrypt.Net;
using MediRecords.Dto.LoginDtos;
using MediRecords.Utility;

namespace MediRecords.Services.AuthServices;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepo;
    private readonly IConfiguration _config;

    public AuthService(IAuthRepository authRepo, IConfiguration config)
    {
        _authRepo = authRepo;
        _config = config;
    }

    /// <summary>
    /// Verifies user credentials, generates authentication tokens, and logs the login activity.
    /// </summary>
    /// <param name="dto">The login credentials provided by the user.</param>
    /// <returns>A LoginResponseDto containing tokens if successful; otherwise, null.</returns>
    public async Task<LoginResponseDto?> LoginUser(LoginRequestDto dto)
    {
        // Fetch the user from the database by their email
        var user = await _authRepo.GetUserByEmailAsync(dto.Email);
        
        // Verify the user exists and the provided password matches the hashed password
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password)) 
        {
            return null;
        }

        // Generate the secure tokens for the session
        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        // Record the login action in the audit logs
        await SaveAuditLog(user.UserId, "Login");

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expires = DateTime.UtcNow.AddHours(1)
        };
    }

    /// <summary>
    /// Records a specific user action into the system audit trail.
    /// </summary>
    /// <param name="userId">The ID of the user performing the action.</param>
    /// <param name="action">A description of the action being performed.</param>
    public async Task SaveAuditLog(int userId, string action)
    {
        await _authRepo.AddAuditLogAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            TimeStamp = DateTime.Now
            
        });
    }

    /// <summary>
    /// Creates a JSON Web Token (JWT) containing user identity claims and a secure signature.
    /// </summary>
    /// <param name="user">The user entity for which the token is being generated.</param>
    /// <returns>A serialized JWT string.</returns>
    public string GenerateJwtToken(User user)
    {
        // Define the identity claims to be stored in the token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name,user.Name)

        };
        if (!string.IsNullOrWhiteSpace(user.RoleIdNavigation?.Name))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.RoleIdNavigation.Name));
        }

        // Set up the security key and signing credentials
        var keyStr = _config["Jwt:Key"] ?? "SecretKeyWithAtLeast32CharactersLong123!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Create the token with the specified configuration and expiration
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically strong random string to be used as a refresh token.
    /// </summary>
    /// <returns>A Base64 encoded random string.</returns>
    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public void EnsurePrimaryProviderAccess(Patient patient, int loggedInUserId)
    {
        if(patient.PrimaryProviderId == null)
        {
            throw new UnauthorizedAccessException(Constant.PatientMessages.NoSuchPrimaryProvider);
        }

        if(patient.PrimaryProviderId != loggedInUserId)
        {
            throw new UnauthorizedAccessException(Constant.PatientMessages.UnauthorizedAccess);
        }
    }
}