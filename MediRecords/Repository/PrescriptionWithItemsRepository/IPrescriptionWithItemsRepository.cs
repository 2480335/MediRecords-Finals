using MediRecords.Domain.Entities;
using MediRecords.Dto.PrescriptionWithItemsDtos;

namespace MediRecords.Repository.PrescriptionWithItemsRepository;

public interface IPrescriptionWithItemsRepository
{
    Task<PrescriptionWithItemsResponseDto> CreatePrescriptionWithItemsAsync(CreatePrescriptionWithItemsRequestDto request);
    Task<PrescriptionWithItemsResponseDto?> GetPrescriptionWithItemsByIdAsync(int prescriptionId);
    Task<IEnumerable<PrescriptionWithItemsResponseDto>> GetAllPrescriptionsWithItemsAsync();
    Task<PrescriptionWithItemsResponseDto> UpdatePrescriptionWithItemsAsync(int prescriptionId, UpdatePrescriptionWithItemsRequestDto request);
    Task<bool> DeletePrescriptionWithItemsAsync(int prescriptionId);
    Task<Encounter?> GetEncounterByIdAsync(int encounterId);
}
