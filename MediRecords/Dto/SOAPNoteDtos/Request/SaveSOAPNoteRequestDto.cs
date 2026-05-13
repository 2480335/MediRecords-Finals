using System.ComponentModel.DataAnnotations;
using MediRecords.Utility;

namespace MediRecords.Dto.SOAPNoteDtos.Request;

public class SaveSOAPNoteRequestDto
{
    [Required]
    public int EncounterId { get; set; }
    // ── S - Subjective 
    [Required(ErrorMessage = Constant.SOAPNoteMessages.HPIRequired)]
    public string HPI { get; set; } = string.Empty;
    public string? ROS { get; set; }

    // ── O - Objective 
    public string? ExamFindings { get; set; }
    public string? Observations { get; set; }

    // ── A - Assessment 
    public string? Assessment { get; set; }

    // ── P - Plan 
    public string? Plan { get; set; }

    public bool IsDraft { get; set; } = true;
}