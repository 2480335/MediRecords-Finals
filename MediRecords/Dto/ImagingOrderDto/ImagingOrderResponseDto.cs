using System;
using MediRecords.Domain.Enums;

namespace MediRecords.Dto.ImagingOrderDto;

public class ImagingOrderResponseDto
{
    public int ImagingOrderId { get; set; }
    public int EncounterId { get; set; }
    public ImagingOrderStudyType? StudyType { get; set; }
    public string Notes { get; set; } = null!;
    public DateTime OrderedDate { get; set; }
    public bool Status { get; set; }
    public List<ImagingReportResponseDto> Reports { get; set; } = new();

}
