using System;

namespace MediRecords.Dto.ProblemListDtos;

public class ProblemCreateRequestDto
{
    public required string Diagnosis { get; set; }
    public DateTime StartDate { get; set; }
    public required string Status { get; set; } // Active, Resolved, Chronic
    public DateTime? EndDate { get; set; }
}
