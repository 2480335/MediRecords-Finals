using System;
using MediRecords.Dto.ImagingReportDto;

namespace MediRecords.Services.ImagingReportServices;

public interface IImagingReportServices
{
    Task CreateReportAsync(int ImagingOrderID,ImagingReportRequestDto dto);

}
