using MediRecords.Domain.Entities;
using MediRecords.Dto.LabOrderDtos;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.LabOrderRepository;

public class LabOrderRepository : ILabOrderRepository
{
    private readonly MediRecordsDbContext _context;

    public LabOrderRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<LabOrder> CreateLabOrderAsync(LabOrder labOrder)
    {
        _context.LabOrders.Add(labOrder);
        await _context.SaveChangesAsync();
        return labOrder;
    }

    public async Task<LabOrder?> GetLabOrderByIdAsync(int labOrderId)
    {
        return await _context.LabOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(lo => lo.LabOrderId == labOrderId);
    }

    public async Task<IEnumerable<LabOrder>> GetLabOrdersByEncounterIdAsync(int encounterId)
    {
        return await _context.LabOrders
            .AsNoTracking()
            .Where(lo => lo.EncounterId == encounterId)
            .OrderByDescending(lo => lo.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<LabOrder>> GetLabOrdersAsync(LabOrderFilterRequestDto filter)
    {
        var query = _context.LabOrders.AsNoTracking().AsQueryable();

        if (filter == null)
        {
            return await query.OrderByDescending(lo => lo.OrderDate).ToListAsync();
        }

        if (filter.EncounterId.HasValue)
            query = query.Where(lo => lo.EncounterId == filter.EncounterId.Value);

        if (filter.OrderedBy.HasValue)
            query = query.Where(lo => lo.OrderedBy == filter.OrderedBy.Value);

        if (filter.Status.HasValue)
            query = query.Where(lo => lo.Status == filter.Status.Value);

        if (filter.OrderDate.HasValue)
            query = query.Where(lo => lo.OrderDate.Date == filter.OrderDate.Value.Date);

        if (!string.IsNullOrWhiteSpace(filter.TestJson))
            query = query.Where(lo => lo.TestJson.Contains(filter.TestJson));

        return await query.OrderByDescending(lo => lo.OrderDate).ToListAsync();
    }

    public async Task<LabOrder> UpdateLabOrderAsync(LabOrder labOrder)
    {
        _context.LabOrders.Update(labOrder);
        await _context.SaveChangesAsync();
        return labOrder;
    }

    public async Task<int> GetLabOrderCountAsync(DateTime fromDate, DateTime toDate)
    {
        var from = fromDate.Date;
        var to = toDate.Date.AddDays(1).AddTicks(-1);
        return await _context.LabOrders
            .Where(lo => lo.OrderDate >= from && lo.OrderDate <= to)
            .CountAsync();
    }
}
