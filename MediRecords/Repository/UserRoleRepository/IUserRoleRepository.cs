using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.UserRoleRepository;

public interface IUserRoleRepository
{
    Task<bool> RoleExistsAsync(int roleId);
    Task<IEnumerable<UserRole>> GetAllAsync();
}
