namespace MediRecords.Dto.SOAPNoteDtos.Response;

public class SOAPNoteResponseDto
{
    public int NoteId { get; set; }
    public int EncounterId { get; set; }

    // ── S - Subjective 
    public string HPI { get; set; } = string.Empty;
    public string? ROS { get; set; }

    // ── O - Objective 
    public string? ExamFindings { get; set; }
    public string? Observations { get; set; }

    // ── A - Assessment 
    public string? Assessment { get; set; }

    // ── P - Plan 
    public string? Plan { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}