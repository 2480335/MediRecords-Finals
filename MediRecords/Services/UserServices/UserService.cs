using System;
using System.Diagnostics;
using MediRecords.Domain.Entities;
using MediRecords.Dto.UserDtos;
using MediRecords.Repository.UserRepo;
using MediRecords.Utility;
using System.Text.RegularExpressions;
using MediRecords.Repository.UserRoleRepository;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using NuGet.Packaging.Signing;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MediRecords.Services.UserServices;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    public UserService(IUserRepository userRepository, IUserRoleRepository userRoleRepository)
    {
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
    }

    /// <summary>
    /// Checks the user's data, hashes the password, and saves the user to the database.
    /// </summary>
    /// <param name="requestDto">The data provided for registration.</param>
    public async Task RegisterUserAsync(UserRegisterRequestDto requestDto)
    {
        // Check if the request (dto) exists
        if (requestDto == null)
        {
            throw new MediRecordsException(Constant.RequestNull);
        }

        // Make sure all required information is filled in
        if (string.IsNullOrWhiteSpace(requestDto.Password) ||
           string.IsNullOrWhiteSpace(requestDto.Email) ||
           string.IsNullOrWhiteSpace(requestDto.Name) ||
           requestDto.RoleId <= 0)
        {
            throw new MediRecordsException(Constant.RequiredFields);
        }

        // Check whether the role exists or not
        bool roleExists = await _userRoleRepository.RoleExistsAsync(requestDto.RoleId);
        if(!roleExists)
        {
            throw new MediRecordsException(Constant.InvalidRoleId);
        }

        // Validate Email
        var emailResult = EmailHelper.ValidateEmail(requestDto.Email);
        if (!emailResult.IsValid)
        {
            throw new MediRecordsException(Constant.InvalidEmailFormat);
        }

        // Validate password
        var passwordResult = PasswordHelper.ValidatePassword(requestDto.Password);
        if (!passwordResult.IsValid)
        {
            throw new MediRecordsException(Constant.WeakPassword);
        }

        // Hash the password to keep it safe in the databse
        requestDto.Password = BCrypt.Net.BCrypt.HashPassword(requestDto.Password);

        // Map the Dto to Domain (User) entity
        var user = requestDto.ToUserRegisterRequest();

        // Pass the data to the repository to get saved
        await _userRepository.RegisterUserAsync(user);
    }

    /// <summary>
    /// Retrieves all users and transforms the domain models into presentation-ready DTOs.
    /// </summary>
    /// <returns>A collection of UserViewDto objects.</returns>
    public async Task<IEnumerable<UserViewDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();

        return users.Select(UserViewDto.FromEntity);
    }

    /// <summary>
    /// Retrieves all physicians for dropdown selection.
    /// </summary>
    public async Task<IEnumerable<ProviderLookupDto>> GetAllProvidersAsync()
    {
        var providers = await _userRepository.GetAllProvidersAsync();

        return providers.Select(p => new ProviderLookupDto
        {
            ProviderId = p.UserId,
            Name = p.Name,
            Email = p.Email
        });
    }

    /// <summary>
    /// Fetches a specific user by ID and converts the entity to a DTO for the API.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The populated UserViewDto if found; otherwise, null.</returns>
    public async Task<UserViewDto?> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);

        return user != null ? UserViewDto.FromEntity(user) : null;
    }
    public async Task<UserUpdateResponseDto> UpdateUser(UserUpdateRequestDto request)
    {
        // Check if the request exists
        if (request == null)
            throw new MediRecordsException(Constant.UserUpdate.UpdateUserRequest);

        // Validate that the UserID is a positive number
        if (request.UserID <= 0)
            throw new ArgumentException(Constant.UserUpdate.InvalidUserId, nameof(request.UserID));

        // Validate if the userID not found in the table
        var user = await _userRepository.GetUserByIdAsync(request.UserID);
        if (user == null)
            throw new MediRecordsException(Constant.UserUpdate.UserNotFound);

        // Validate that the user's name is provided
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException(Constant.UserUpdate.NameRequired, nameof(request.Name));

        // Validate that the phone number is provided
        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new ArgumentException(Constant.UserUpdate.PhoneRequired, nameof(request.Phone));

        // Validate that the RoleID is valid
        if (request.RoleID <= 0)
            throw new MediRecordsException(Constant.UserUpdate.InvalidRoleId);

        bool roleExists = await _userRoleRepository.RoleExistsAsync(request.RoleID);
        if(!roleExists)
        {
            throw new MediRecordsException(Constant.InvalidRoleId);
        }

        // Delegate persistence and data update logic to the repository layer
        return await _userRepository.UpdateUser(request);
    }

    public async Task<(bool Success, string Message, int StatusCode)> ForgotPasswordAsync(UserForgotPasswordDto model)
    {
        try
        {
            if (!IsValidEmail(model.Email))
                return (false, Constant.Messages.EmailInvalid, 400);
            // Validate password match
            if (model.NewPassword != model.ConfirmPassword)
                return (false, Constant.Messages.PasswordMismatch, 400);

            // Validate password strength
            if (!IsValidPassword(model.NewPassword))
                return (false, Constant.Messages.WeakPassword, 400);

            // Check user exists
            var user = await _userRepository.GetByEmailAsync(model.Email);
            if (user == null)
                return (false, Constant.Messages.UserNotFound, 404);

            // Hash and update password
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _userRepository.UpdateAsync(user);

            return (true, Constant.Messages.PasswordUpdated, 200);
        }
        catch (Exception)
        {
            // Log ex here if a logger is injected (recommended)
            return (false, Constant.Messages.SomethingWentWrong, 500);
        }
    }

    private bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;

        // Min 8 chars, at least one uppercase, one lowercase, one digit, one special char
        var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";
        return Regex.IsMatch(password, pattern);
    }
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
    
    /// <summary>
    /// Fetches a specific user by ID and change the status from Active to InActive.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task SoftDeleteUserByIdAsync(int id)
    {
        if(id < 0)
        {
            throw new ArgumentException(Constant.InvalidUserId);
        }
       await _userRepository.SoftDeleteUserByIdAsync(id);
    }
}