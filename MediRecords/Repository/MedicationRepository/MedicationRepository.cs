using MediRecords.Domain.Entities;
using MediRecords.Domain.Enums;
using MediRecords.Dto.MedicationListDtos;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.MedicationRepository;

public class MedicationRepository : IMedicationRepository
{
    private readonly MediRecordsDbContext _context;

    public MedicationRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MedicationList>> GetMedicationListsAsync(MedicationListRequestDto filter)
    {
        var query = _context.MedicationLists.AsNoTracking().AsQueryable();

        if (filter == null)
        {
            return await query.OrderBy(x => x.PatientId).ToListAsync();
        }

        if (filter.PatientId.HasValue)
            query = query.Where(x => x.PatientId == filter.PatientId.Value);

        if (filter.MedId.HasValue)
            query = query.Where(x => x.MedId == filter.MedId.Value);

        if(filter.PatientName != null)
            query = query.Where(x => x.PatientIdNavigation != null && x.PatientIdNavigation.Name.Contains(filter.PatientName));

        if (!string.IsNullOrWhiteSpace(filter.DrugName))
            query = query.Where(x => x.DrugName.Contains(filter.DrugName));

        if (!string.IsNullOrWhiteSpace(filter.Dose))
            query = query.Where(x => x.Dose == filter.Dose);

        if (!string.IsNullOrWhiteSpace(filter.Frequency))
            query = query.Where(x => x.Frequency.Contains(filter.Frequency));

        if (!string.IsNullOrWhiteSpace(filter.Route))
            query = query.Where(x => x.Route != null && x.Route.Contains(filter.Route));

        if (filter.StartDate.HasValue)
            query = query.Where(x => x.StartDate >= filter.StartDate.Value.Date);

        if (filter.EndDate.HasValue)
            query = query.Where(x => x.EndDate.HasValue && x.EndDate.Value.Date <= filter.EndDate.Value.Date);

        if (!string.IsNullOrEmpty(filter.Status))
        {
            var medicationStatus = Enum.Parse<MedicationStatus>(filter.Status, ignoreCase: true);
            query = query.Where(x => x.Status == medicationStatus);
        }

        return await query
        .OrderByDescending(x => x.StartDate)
        .Include(x => x.PatientIdNavigation)
        .ToListAsync();
    }
}
