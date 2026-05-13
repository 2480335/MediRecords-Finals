using MediRecords.Domain.Entities;
using MediRecords.Dto.LoginDtos;
using MediRecords.Dto.UserDtos;

namespace MediRecords.Services.AuthServices;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginUser(LoginRequestDto dto);
    Task SaveAuditLog(int userId, string action);
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
    void EnsurePrimaryProviderAccess(Patient patient, int loggedInUserId);

}