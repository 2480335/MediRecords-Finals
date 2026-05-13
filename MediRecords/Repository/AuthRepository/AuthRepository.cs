using Microsoft.EntityFrameworkCore;

using MediRecords.Domain.Entities;

namespace MediRecords.Repositories;
public class AuthRepository : IAuthRepository
{
    private readonly MediRecordsDbContext _context;

    public AuthRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
        .Include(u => u.RoleIdNavigation)
        .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAuditLogAsync(AuditLog log)
    {
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}