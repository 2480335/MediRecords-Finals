using MediRecords.Domain.Entities;

namespace MediRecords.Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task AddAuditLogAsync(AuditLog log);
}