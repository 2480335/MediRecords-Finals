using System;
using MediRecords.Domain.Entities;
using MediRecords.Dto.UserDtos;

namespace MediRecords.Repository.UserRepo;

public interface IUserRepository
{
    Task RegisterUserAsync(User user);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<IEnumerable<User>> GetAllProvidersAsync();
    Task<User?> GetUserByIdAsync(int id);

    public Task<UserUpdateResponseDto> UpdateUser(UserUpdateRequestDto request);

    Task<User> GetByEmailAsync(string email);
    Task UpdateAsync(User user);

    Task SoftDeleteUserByIdAsync(int id);
}
