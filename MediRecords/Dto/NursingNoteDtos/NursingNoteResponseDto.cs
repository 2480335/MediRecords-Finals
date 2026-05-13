namespace MediRecords.Dto.NursingNoteDtos;

public class NursingNoteResponseDto
{
    public int NursingNoteId { get; set; }
    public int EncounterId { get; set; }
    public string Notes { get; set; } = null!;
    public string RecordedBy { get; set; } = null!;
    public DateTime RecordedDate { get; set; }
}
