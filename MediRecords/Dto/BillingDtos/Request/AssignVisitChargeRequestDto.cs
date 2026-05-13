using System;
using MediRecords.Utility;
using System.ComponentModel.DataAnnotations;
using Humanizer;
namespace MediRecords.Dto.BillingDtos.Request;

public class AssignVisitChargeRequestDto
{
    [Required(ErrorMessage = Constant.BillingMessages.EncounterIdRequired)]
    public int EncounterId { get; set; }

    [Required(ErrorMessage = Constant.BillingMessages.CodeIdRequired)]
    public int CodeId { get; set; }

    public decimal? Amount { get; set; }
}
