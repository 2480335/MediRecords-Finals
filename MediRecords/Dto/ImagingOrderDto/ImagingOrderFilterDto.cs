using System;
using MediRecords.Domain.Enums;

namespace MediRecords.Dto.ImagingOrderDto;

public class ImagingOrderFilterDto
{
    public int? ImagingOrderID { get; set; }
    public int? EncounterID { get; set; }
    public ImagingOrderStudyType? StudyType { get; set; }
    public string? Notes { get; set; }
    public DateTime? OrderedDate { get; set; }
    public bool? Status { get; set; }
    public bool HasAnyValue()
    {
        return ImagingOrderID.HasValue
            || EncounterID.HasValue
            || StudyType.HasValue
            || !string.IsNullOrWhiteSpace(Notes)
            || OrderedDate.HasValue
            || Status.HasValue;
    }
}
