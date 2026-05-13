using System;
using System.ComponentModel.DataAnnotations;

namespace MediRecords.Dto.ImagingReportDto;

public class ImagingReportRequestDto
{
    public required string Findings { get; set; }
    public string Impression { get; set; } = null!;
    public IFormFile? ReportFile {get;set;}
}
