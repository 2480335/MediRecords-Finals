using System.ComponentModel.DataAnnotations;
using MediRecords.Utility;

namespace MediRecords.Dto.BillingDtos.Request;

public class MarkChargesBilledRequestDto
{
    [Required(ErrorMessage = Constant.BillingMessages.ChargeIdsRequired)]
    [MinLength(1, ErrorMessage = Constant.BillingMessages.ChargeIdsRequired)]
    public List<int> ChargeIds { get; set; } = new();
}