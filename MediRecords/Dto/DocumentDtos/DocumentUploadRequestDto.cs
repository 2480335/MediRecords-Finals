using System.ComponentModel.DataAnnotations;

namespace MediRecords.Dto.DocumentDtos;

public class DocumentUploadRequestDto
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public int EncounterId { get; set; }

    [Required]
    public string DocType { get; set; } = null!; // "Referral", "Consent", "Report", "Photo"

    public bool ProviderOnlyVisibility { get; set; } = false;

    [Required]
    public IFormFile File { get; set; } = null!;
}
