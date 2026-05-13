using System.ComponentModel.DataAnnotations;

namespace MediRecords.Dto.NursingNoteDtos;

public class NursingNoteUpdateRequestDto
{
    [StringLength(50, ErrorMessage = "Notes must be 50 characters or fewer.")]
    [Required]
    public string Notes { get; set; } = null!;


}
