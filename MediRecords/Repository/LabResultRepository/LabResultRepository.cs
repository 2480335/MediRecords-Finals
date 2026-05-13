using MediRecords.Domain.Entities;
using MediRecords.Dto.LabResultDtos;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.LabResultRepository;

public class LabResultRepository : ILabResultRepository
{
    private readonly MediRecordsDbContext _context;

    public LabResultRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<LabResult> CreateLabResultAsync(LabResult labResult)
    {
        _context.LabResults.Add(labResult);
        await _context.SaveChangesAsync();
        return labResult;
    }

    public async Task<LabResult?> GetLabResultByIdAsync(int resultId)
    {
        return await _context.LabResults
            .AsNoTracking()
            .FirstOrDefaultAsync(lr => lr.ResultId == resultId);
    }

    public async Task<IEnumerable<LabResult>> GetLabResultsByLabOrderIdAsync(int labOrderId)
    {
        return await _context.LabResults
            .AsNoTracking()
            .Where(lr => lr.LabOrderId == labOrderId)
            .OrderByDescending(lr => lr.ResultDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabResult>> GetLabResultsAsync(LabResultRequestDto filter)
    {
        var query = _context.LabResults.AsNoTracking().AsQueryable();

        if (filter == null)
        {
            return await query.OrderByDescending(lr => lr.ResultDate).ToListAsync();
        }

        if (filter.LabOrderId.HasValue)
            query = query.Where(lr => lr.LabOrderId == filter.LabOrderId.Value);

        if (filter.Status.HasValue)
            query = query.Where(lr => lr.Status == filter.Status.Value);

        if (filter.ResultDate.HasValue)
            query = query.Where(lr => lr.ResultDate.Date == filter.ResultDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(filter.ResultJson))
            query = query.Where(lr => lr.ResultJson != null && lr.ResultJson.Contains(filter.ResultJson));

        return await query.OrderByDescending(lr => lr.ResultDate).ToListAsync();
    }
}
