using System.ComponentModel.DataAnnotations;

namespace MediRecords.Dto.VitalSignDtos;

public class VitalSignCreateRequestDto
{
    [Required]
    public int EncounterId { get; set; }

    public string? Bp { get; set; }

    public string? Hr { get; set; }

    public string? Temp { get; set; }

    public string? SpO2 { get; set; }

    public double? Height { get; set; }

    public double? Weight { get; set; }
}