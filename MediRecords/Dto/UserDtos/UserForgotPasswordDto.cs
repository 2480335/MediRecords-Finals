using System.ComponentModel.DataAnnotations;
using MediRecords.Utility;

namespace MediRecords.Dto.UserDtos;

public class UserForgotPasswordDto
{
    [Required(ErrorMessage = Constant.Messages.EmailRequired)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = Constant.Messages.NewPasswordRequired)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = Constant.Messages.ConfirmPasswordRequired)]
    public string ConfirmPassword { get; set; } = string.Empty;
}