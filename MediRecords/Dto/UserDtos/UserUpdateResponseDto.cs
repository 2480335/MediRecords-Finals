using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Dto.UserDtos;

public class UserUpdateResponseDto
{
    public int UserID { get; init; }
    public string Name { get; set; }
    public int RoleID { get; set; }
    public string Email { get; init; }
    public string Phone { get; set; }
    public bool Status { get; set; }
}

public static class UserUpdateResponseExtension
{
    public static UserUpdateResponseDto ToUserUpdateResponse(this User user)
    {
        return new UserUpdateResponseDto
        {
            UserID = user.UserId,
            Name = user.Name,
            RoleID = user.RoleId,
            Phone = user.Phone,
            Email=user.Email,
            Status = user.Status
        };
    }
}