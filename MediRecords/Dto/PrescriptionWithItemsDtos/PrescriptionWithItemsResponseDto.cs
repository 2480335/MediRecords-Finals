using System.ComponentModel.DataAnnotations;
using MediRecords.Domain.Enums;

namespace MediRecords.Dto.PrescriptionWithItemsDtos;

public class PrescriptionItemWithPrescriptionResponseDto
{
    public int ItemId { get; set; }
    public int PrescriptionId { get; set; }
    public string DrugName { get; set; } = null!;
    public string? Dose { get; set; }
    public string? Frequency { get; set; }
    public string? Route { get; set; }
    public int DurationDays { get; set; }
    public string? Instructions { get; set; }
}

public class PrescriptionWithItemsResponseDto
{
    public int PrescriptionId { get; set; }
    public int EncounterId { get; set; }
    public int ProviderId { get; set; }
    public DateOnly CreatedDate { get; set; }
    public string Status { get; set; } = null!; // "Draft" or "Issued"
    public List<PrescriptionItemWithPrescriptionResponseDto> PrescriptionItems { get; set; } = new();
}

public class UpdatePrescriptionWithItemsRequestDto
{
    [Required]
    public int ProviderId { get; set; }

    public string Status { get; set; } = "Draft"; // "Draft" or "Issued"

    [Required]
    public List<PrescriptionItemCreateDto> PrescriptionItems { get; set; } = new();
}
