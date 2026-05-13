using System;
using MediRecords.Domain.Entities;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.UserRoleRepository;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly MediRecordsDbContext _context;
    public UserRoleRepository(MediRecordsDbContext context)
    {
        _context = context;
    }
    public async Task<bool> RoleExistsAsync(int roleId)
    {
        return await _context.UserRoles.AnyAsync(r => r.RoleId == roleId);
    }

    public async Task<IEnumerable<UserRole>> GetAllAsync()
    {
        return await _context.UserRoles
            .AsNoTracking()
            .OrderBy(r => r.RoleId)
            .ToListAsync();
    }
}
