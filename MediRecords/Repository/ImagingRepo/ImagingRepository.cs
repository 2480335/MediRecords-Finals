using System;
using MediRecords.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace MediRecords.Repository.ImagingRepo;

public class ImagingRepository : IImagingRepository
{
    private readonly MediRecordsDbContext _context;

    public ImagingRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<bool> OrderExistsAsync(int imagingOrderId)
    {
        return await _context.ImagingOrders
            .AnyAsync(o => o.ImagingOrderId == imagingOrderId);
    }

    public async Task<IEnumerable<ImagingReport>> GetReportsByOrderIdAsync(int imagingOrderId)
    {
        return await _context.ImagingReports
            .Where(r => r.ImagingOrderId == imagingOrderId)
            .OrderByDescending(r => r.ReportDate)
            .ToListAsync();
    }
}
