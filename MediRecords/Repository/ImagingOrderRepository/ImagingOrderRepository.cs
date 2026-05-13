using System;
using MediRecords.Domain.Entities;
using MediRecords.Dto.ImagingOrderDto;
using MediRecords.Utility;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.ImagingOrderRepository;

public class ImagingOrderRepository : IImagingOrderRepository
{
    private readonly MediRecordsDbContext _context;
    public ImagingOrderRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Validates the existence and status of an encounter before asynchronously adding a new imaging order.
    /// </summary>
    /// <param name="order">The imaging order entity to be added.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the associated encounter does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the encounter is closed (Status is 0).</exception>
    public async Task AddAsync(ImagingOrder order)
    {
        var encounter = await _context.Encounters
            .Select(e => new { e.EncounterId, e.Status })
            .FirstOrDefaultAsync(e => e.EncounterId == order.EncounterId);

        if (encounter == null)
        {
            throw new KeyNotFoundException(Constant.EncounterNotFound);
        }

        if (encounter.Status == 0)
        {
            throw new InvalidOperationException(Constant.EncounterClosed);
        }

        bool imagingOrderExists = await _context.ImagingOrders.AnyAsync(o => o.EncounterId == order.EncounterId && o.StudyType == order.StudyType && o.Status);


        if (imagingOrderExists)
        {
            throw new InvalidOperationException(Constant.ImagingOrderAlreadyExists);
        }
        await _context.ImagingOrders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// fatches imaging orders based on the given filter from db.
    /// </summary>
    /// <param name="filter">These filters will be used to fatch imging order</param>
    /// <returns>Retuns the imaging order based on the filter</returns>
    public async Task<List<ImagingOrder>> GetAllAsync(ImagingOrderFilterDto filter)
    {
        var query = _context.ImagingOrders.Include(x => x.ImagingReports).AsNoTracking().AsQueryable();

        if (filter.ImagingOrderID.HasValue)
            query = query.Where(x => x.ImagingOrderId == filter.ImagingOrderID);

        if (filter.EncounterID.HasValue)
            query = query.Where(x => x.EncounterId == filter.EncounterID);

        if (filter.StudyType.HasValue)
        {
            query = query.Where(x => x.StudyType == filter.StudyType.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Notes))
            query = query.Where(x => x.Notes.Contains(filter.Notes));

        if (filter.OrderedDate.HasValue)
            query = query.Where(x => x.OrderedDate.Date == filter.OrderedDate.Value.Date);

        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status);

        return await query.ToListAsync();

    }
}
