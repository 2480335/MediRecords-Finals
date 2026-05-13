using MediRecords.Dto.PrescriptionWithItemsDtos;

namespace MediRecords.Services.PrescriptionWithItemsServices;

public interface IPrescriptionWithItemsService
{
    Task<PrescriptionWithItemsResponseDto> CreatePrescriptionWithItemsAsync(CreatePrescriptionWithItemsRequestDto request);
    Task<PrescriptionWithItemsResponseDto?> GetPrescriptionWithItemsByIdAsync(int prescriptionId);
    Task<IEnumerable<PrescriptionWithItemsResponseDto>> GetAllPrescriptionsWithItemsAsync();
    Task<PrescriptionWithItemsResponseDto> UpdatePrescriptionWithItemsAsync(int prescriptionId, UpdatePrescriptionWithItemsRequestDto request);
    Task<bool> DeletePrescriptionWithItemsAsync(int prescriptionId);
}
