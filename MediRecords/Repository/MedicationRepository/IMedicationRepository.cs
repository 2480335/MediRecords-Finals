using MediRecords.Domain.Entities;
using MediRecords.Dto.MedicationListDtos;

namespace MediRecords.Repository.MedicationRepository;

public interface IMedicationRepository
{
    Task<IEnumerable<MedicationList>> GetMedicationListsAsync(MedicationListRequestDto filter);
}
