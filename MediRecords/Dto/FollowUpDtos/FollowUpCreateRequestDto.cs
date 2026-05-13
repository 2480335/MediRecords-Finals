using System;

namespace MediRecords.Dto.FollowUpDtos;

public class FollowUpCreateRequestDto
{
    public DateTime RecommendedDate { get; set; }
    public string Notes { get; set; } = null!;
}
