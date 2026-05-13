using MediRecords.Models;
using MediRecords.Dto.VitalSignDtos;
using MediRecords.Repository.VitalSignRepository;
using MediRecords.Utility;
using MediRecords.Domain.Enums;

namespace MediRecords.Services.VitalSignServices;

public class VitalSignService : IVitalSignService
{
    private readonly IVitalSignRepository _vitalSignRepository;

    public VitalSignService(IVitalSignRepository vitalSignRepository)
    {
        _vitalSignRepository = vitalSignRepository;
    }

    public async Task<VitalSignCreateResponseDto> CreateVitalSignsAsync(VitalSignCreateRequestDto request, string NurseId)
    {
        //  Validation: Ensure the encounter exists and is open
        var encounter = await _vitalSignRepository.GetEncounterByIdAsync(request.EncounterId);
        
        if (encounter == null)
        {
            throw new MediRecordsException("Encounter not found");
        }

        if (encounter.Status != EncounterStatus.Open)
        {
            throw new MediRecordsException("Encounter is closed");
        }

        // Auto-calculate BMI
        double? calculatedBmi = null;
        if (request.Height.HasValue && request.Weight.HasValue && request.Height.Value > 0)
        {
            // BMI Formula: weight (kg) / [height (m)]^2
            calculatedBmi = Math.Round(request.Weight.Value / Math.Pow(request.Height.Value / 100, 2), 2);
        }

        //  Mapping: Create a single Vitals record (One row in DB)
        var vitalsEntry = new VitalSign
        {
            EncounterId = request.EncounterId,
            BP = request.Bp,
            
            // Converting strings from DTO to double? for the VitalSign model
            HR = double.TryParse(request.Hr, out double hrVal) ? hrVal : null,
            Temp = double.TryParse(request.Temp, out double tempVal) ? tempVal : null,
            SpO2 = double.TryParse(request.SpO2, out double spo2Val) ? spo2Val : null,
            
            Height = request.Height,
            Weight = request.Weight,
            BMI = calculatedBmi,
            RecordedDate = DateTime.UtcNow,
            RecordedBy = NurseId
        };

        //  Persistence: Save the record via the repository
        var savedVitals = await _vitalSignRepository.AddAsync(vitalsEntry);

        //  Return Response DTO
        return new VitalSignCreateResponseDto
        {
            VitalId = savedVitals.VitalsId,
            Bmi = calculatedBmi
        };
    }
}