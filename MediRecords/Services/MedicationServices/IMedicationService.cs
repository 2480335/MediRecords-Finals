using MediRecords.Dto.MedicationListDtos;

namespace MediRecords.Services.MedicationServices;

public interface IMedicationService
{
    Task<IEnumerable<MedicationListResponseDto>> GetMedicationListsAsync(MedicationListRequestDto filter);
}
