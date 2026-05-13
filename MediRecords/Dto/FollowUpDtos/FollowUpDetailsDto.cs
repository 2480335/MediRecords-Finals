using System;

namespace MediRecords.Dto.FollowUpDtos;

public class FollowUpDetailsDto
{
    public int FollowupId { get; set; }
    public int EncounterId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = null!;
    public DateTime RecommendedDate { get; set; }
    public string Notes { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
}
