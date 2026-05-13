using System;
using Humanizer;

namespace MediRecords.Dto.BillingDtos.Response;

public class VisitChargeResponseDto
{
    public int ChargeId { get; set; }
    public int EncounterId { get; set; }
    public int CodeId { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    // false = Unbilled | true = Billed
    public string Status { get; set; }
}