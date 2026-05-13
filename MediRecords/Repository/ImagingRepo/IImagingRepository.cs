using System;
using MediRecords.Domain.Entities;
namespace MediRecords.Repository.ImagingRepo;

public interface IImagingRepository
{
    Task<bool> OrderExistsAsync(int imagingOrderId);
    Task<IEnumerable<ImagingReport>> GetReportsByOrderIdAsync(int imagingOrderId);
}
