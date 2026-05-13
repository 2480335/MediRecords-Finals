using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Dto.UserDtos;

public class UserRegisterRequestDto
{   
    /* This is the data transfer object (DTO) for user registration. It contains the necessary properties required to create a new user in the system. The data is coming from the client while registering */
    public required string Name { get; set; }
    public required int RoleId { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Password { get; set; }
    public required bool Status { get; set; } = true;

    public User ToUserRegisterRequest()
    {
        return new User()
        {
            Name = Name,
            RoleId = RoleId,
            Email = Email,
            Phone = Phone ?? "",
            Password = Password,
            Status = Status
        };
    }
}
