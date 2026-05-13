using System.ComponentModel.DataAnnotations;
using MediRecords.Utility;

namespace MediRecords.Dto.BillingDtos.Request;

public class UpdateVisitChargeRequestDto
{
    [Required(ErrorMessage = Constant.BillingMessages.AmountRequired)]
    public decimal Amount { get; set; }
}