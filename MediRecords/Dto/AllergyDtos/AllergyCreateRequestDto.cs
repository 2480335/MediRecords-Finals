using System;

namespace MediRecords.Dto.AllergyDtos;

public class AllergyCreateRequestDto
{
    public required string Allergen { get; set; }
    public string? Reaction { get; set; }
    public string? Severity { get; set; }
}
