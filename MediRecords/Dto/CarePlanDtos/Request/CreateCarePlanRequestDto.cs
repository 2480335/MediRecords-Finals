using System.ComponentModel.DataAnnotations;
using MediRecords.Utility;

namespace MediRecords.Dto.CarePlanDtos.Request;

public class CreateCarePlanRequestDto
{
    [Required(ErrorMessage = Constant.CarePlanMessages.PatientIdRequired)]
    public int PatientId { get; set; }

    [Required(ErrorMessage = Constant.CarePlanMessages.GoalsRequired)]
    [MinLength(1, ErrorMessage = Constant.CarePlanMessages.GoalsRequired)]
    public List<string> Goals { get; set; } = new();

    [Required(ErrorMessage = Constant.CarePlanMessages.InstructionsRequired)]
    public string Instructions { get; set; } = string.Empty;
    public bool Status { get; set; } = false; 
}