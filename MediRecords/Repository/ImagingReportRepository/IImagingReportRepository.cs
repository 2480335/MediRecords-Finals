using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Repository.ImagingReportRepository;

public interface IImagingReportRepository
{
    Task<string> SaveFileAsync(IFormFile file);
    Task AddAsync(ImagingReport report);

}
