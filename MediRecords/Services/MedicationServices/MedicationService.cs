using MediRecords.Dto.MedicationListDtos;
using MediRecords.Repository.MedicationRepository;

namespace MediRecords.Services.MedicationServices;

public class MedicationService : IMedicationService
{
    private readonly IMedicationRepository _medicationRepository;

    public MedicationService(IMedicationRepository medicationRepository)
    {
        _medicationRepository = medicationRepository;
    }

    public async Task<IEnumerable<MedicationListResponseDto>> GetMedicationListsAsync(MedicationListRequestDto filter)
    {
        filter ??= new MedicationListRequestDto();
        var medications = await _medicationRepository.GetMedicationListsAsync(filter);
        return medications.Select(MedicationListResponseDto.FromEntity);
    }
}
