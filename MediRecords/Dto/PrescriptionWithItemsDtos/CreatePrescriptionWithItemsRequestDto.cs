using System.ComponentModel.DataAnnotations;

namespace MediRecords.Dto.PrescriptionWithItemsDtos;

public class PrescriptionItemCreateDto
{
    [Required]
    [StringLength(100)]
    public string DrugName { get; set; } = null!;

    [StringLength(50)]
    public string? Dose { get; set; }

    [StringLength(50)]
    public string? Frequency { get; set; }

    [StringLength(50)]
    public string? Route { get; set; }

    public int DurationDays { get; set; }

    public string? Instructions { get; set; }
}

public class CreatePrescriptionWithItemsRequestDto
{
    [Required]
    public int EncounterId { get; set; }

    [Required]
    public int ProviderId { get; set; }

    public string Status { get; set; } = "Draft"; // "Draft" or "Issued"

    [Required]
    public List<PrescriptionItemCreateDto> PrescriptionItems { get; set; } = new();
}
