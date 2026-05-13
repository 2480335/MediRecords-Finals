using System;
using MediRecords.Domain.Entities;
using MediRecords.Dto.UserDtos;

namespace MediRecords.Services.UserServices;

public interface IUserService
{
    Task RegisterUserAsync(UserRegisterRequestDto requestDto);

    Task<IEnumerable<UserViewDto>> GetAllUsersAsync();

    Task<IEnumerable<ProviderLookupDto>> GetAllProvidersAsync();

    Task<UserViewDto?> GetUserByIdAsync(int id);

    public Task<UserUpdateResponseDto> UpdateUser(UserUpdateRequestDto request);

    Task <(bool Success, string Message, int StatusCode)> ForgotPasswordAsync(UserForgotPasswordDto model);

    Task SoftDeleteUserByIdAsync(int id);
}
