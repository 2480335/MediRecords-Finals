using MediRecords.Dto.PrescriptionWithItemsDtos;
using MediRecords.Repository.PrescriptionWithItemsRepository;
using MediRecords.Utility;
using MediRecords.Domain.Enums;

namespace MediRecords.Services.PrescriptionWithItemsServices;

public class PrescriptionWithItemsService : IPrescriptionWithItemsService
{
    private readonly IPrescriptionWithItemsRepository _repository;

    public PrescriptionWithItemsService(IPrescriptionWithItemsRepository repository)
    {
        _repository = repository;
    }

    public async Task<PrescriptionWithItemsResponseDto> CreatePrescriptionWithItemsAsync(CreatePrescriptionWithItemsRequestDto request)
    {
        if (request == null)
            throw new MediRecordsException(Constant.RequestNull);

        if(request.EncounterId <= 0)
            throw new MediRecordsException("Invalid encounter ID.");

        var encounter = await _repository.GetEncounterByIdAsync(request.EncounterId);
        if (encounter == null)
            throw new MediRecordsException("Encounter not found.");

        if(request.PrescriptionItems.Any(pi => pi.DurationDays <= 0))
            throw new MediRecordsException("Duration days must be greater than zero for all prescription items.");

        if(request.PrescriptionItems.Any(pi => !string.IsNullOrWhiteSpace(pi.Frequency) && string.IsNullOrWhiteSpace(pi.Route)))
            throw new MediRecordsException("Route is required when frequency is provided for a prescription item.");

        return await _repository.CreatePrescriptionWithItemsAsync(request);
    }

    public async Task<PrescriptionWithItemsResponseDto?> GetPrescriptionWithItemsByIdAsync(int prescriptionId)
    {
        if (prescriptionId <= 0)
            throw new MediRecordsException("Invalid prescription ID.");

        return await _repository.GetPrescriptionWithItemsByIdAsync(prescriptionId);
    }

    public async Task<IEnumerable<PrescriptionWithItemsResponseDto>> GetAllPrescriptionsWithItemsAsync()
    {
        return await _repository.GetAllPrescriptionsWithItemsAsync();
    }

    public async Task<PrescriptionWithItemsResponseDto> UpdatePrescriptionWithItemsAsync(int prescriptionId, UpdatePrescriptionWithItemsRequestDto request)
    {
        if (request == null)
            throw new MediRecordsException(Constant.RequestNull);

        if (prescriptionId <= 0)
            throw new MediRecordsException("Invalid prescription ID.");

        return await _repository.UpdatePrescriptionWithItemsAsync(prescriptionId, request);
    }

    public async Task<bool> DeletePrescriptionWithItemsAsync(int prescriptionId)
    {
        if (prescriptionId <= 0)
            throw new MediRecordsException("Invalid prescription ID.");

        return await _repository.DeletePrescriptionWithItemsAsync(prescriptionId);
    }
}
