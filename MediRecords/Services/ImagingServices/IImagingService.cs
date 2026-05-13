using System;
using MediRecords.Dto.ImagingDtos.Response;

namespace MediRecords.Services.ImagingServices;

public interface IImagingService
{
    Task<(bool Success, string Message, IEnumerable<ImagingReportResponseDto>? Data)>
    GetReportsByOrderIdAsync(int imagingOrderId);
}