using System;
using MediRecords.Domain.Entities;
using MediRecords.Utility;
using MediRecords.Dto.UserDtos;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.UserRepo;

public class UserRepository : IUserRepository
{
    private readonly MediRecordsDbContext _context;
    public UserRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all users from the database, including their associated role information.
    /// </summary>
    /// <returns>A list of User entities with Role navigation properties populated.</returns>
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        // Use AsNoTracking for "Read-Only" operations to improve performance
        // and reduce memory usage in Entity Framework.
        return await _context.Users
            .Include(u => u.RoleIdNavigation)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all users whose role is Physician, used for provider dropdown lookups.
    /// </summary>
    public async Task<IEnumerable<User>> GetAllProvidersAsync()
    {
        return await _context.Users
            .Include(u => u.RoleIdNavigation)
            .Where(u => u.RoleIdNavigation != null && u.RoleIdNavigation.Name == Constant.Physician)
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Finds a specific user by their unique ID.
    /// </summary>
    /// <param name="id">The primary key ID of the user.</param>
    /// <returns>The User entity if found; otherwise, null.</returns>
    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.RoleIdNavigation)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    /// <summary>
    /// Handles the database logic for registering a user, including email uniqueness checks and persistence.
    /// </summary>
    /// <param name="user">The registration request containing user details.</param>
    /// <exception cref="Exception">Thrown when a user with the provided email already exists.</exception>
    public async Task RegisterUserAsync(User user)
    {
        if (user == null)
        {
            throw new ArgumentNullException(Constant.RequestNull);
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email && u.Status == true);
        if (existingUser != null)
        {
            throw new MediRecordsException(Constant.EmailExists);
        }

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task<UserUpdateResponseDto> UpdateUser(UserUpdateRequestDto request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == request.UserID && request.Status == true);

        if (user == null)
        {
            throw new InvalidOperationException(Constant.UserUpdate.UserNotFound);
        }

        // Update allowed fields
        user.Name = request.Name;
        user.Phone = request.Phone;
        user.RoleId = request.RoleID;
        user.Status = request.Status;

        await _context.SaveChangesAsync();

        // Map Domain Entity to Response DTO
        return user.ToUserUpdateResponse();
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Finds a specific user by their unique ID and changes the status from Active to InActive
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task SoftDeleteUserByIdAsync(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == id && u.Status) ?? throw new KeyNotFoundException(Constant.UserUpdate.UserStatus);
        user.Status = false;
        await _context.SaveChangesAsync();
    }
}
