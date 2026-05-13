using System;

namespace MediRecords.Dto.ImagingDtos.Response;

public class ImagingReportResponseDto
{
    public int ReportId { get; set; }
    public int ImagingOrderId { get; set; }
    public string Findings { get; set; }
    public string Impression { get; set; }
    public DateTime ReportDate { get; set; }

    public string Status { get; set; } = string.Empty;
}