using MediRecords.Utility;
using MediRecords.Domain.Entities;

namespace MediRecords.Dto.UserDtos;

public class UserViewDto
{
    public required int UserId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string RoleName { get; set;}
    public required string Status {get;set;}

    /// <summary>
    /// Converts a User entity to a UserViewDto using the central Constant for fallbacks.
    /// </summary>
    public static UserViewDto FromEntity(User user) => new()
    {
        UserId = user.UserId,
        Name = user.Name,
        Email = user.Email,
        Phone = user.Phone,
        RoleName = user.RoleIdNavigation?.Name ?? Constant.Unassigned,
        Status = user.Status ? Constant.Active : Constant.Inactive
    };
}