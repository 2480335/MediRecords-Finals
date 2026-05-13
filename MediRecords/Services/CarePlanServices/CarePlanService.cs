using System.Text.Json;
using MediRecords.Domain.Entities;
using MediRecords.Dto.CarePlanDtos.Request;
using MediRecords.Dto.CarePlanDtos.Response;
using MediRecords.Repository.CarePlanRepo;
using MediRecords.Utility;

namespace MediRecords.Services.CarePlanServices;

public class CarePlanService : ICarePlanService
{
    private readonly ICarePlanRepository _carePlanRepository;

    public CarePlanService(ICarePlanRepository carePlanRepository)
    {
        _carePlanRepository = carePlanRepository;
    }

    public async Task<(bool Success, string Message, CarePlanResponseDto? Data)> CreateCarePlanAsync(
        CreateCarePlanRequestDto dto)
    {
        try
        {
            if (dto.PatientId <= 0)
                return (false, Constant.CarePlanMessages.InvalidPatientId, null);

            if (dto.Goals == null || dto.Goals.Count == 0 ||
                dto.Goals.All(g => string.IsNullOrWhiteSpace(g)))
                return (false, Constant.CarePlanMessages.GoalsRequired, null);

            if (string.IsNullOrWhiteSpace(dto.Instructions))
                return (false, Constant.CarePlanMessages.InstructionsRequired, null);

            var patientExists = await _carePlanRepository.PatientExistsAsync(dto.PatientId);
            if (!patientExists)
                return (false, Constant.CarePlanMessages.PatientNotFound, null);

            var goalsJson = JsonSerializer.Serialize(dto.Goals);

            var carePlan = new CarePlan
            {
                PatientId    = dto.PatientId,
                GoalsJSON    = goalsJson,
                Instructions = dto.Instructions,
                Status       = dto.Status
            };

            var saved = await _carePlanRepository.AddAsync(carePlan);

            var response = new CarePlanResponseDto
            {
                CarePlanId   = saved.CarePlanId,
                PatientId    = saved.PatientId,
                Goals        = JsonSerializer.Deserialize<List<string>>(saved.GoalsJSON) ?? new(),
                Instructions = saved.Instructions,
                Status       = saved.Status ? "Completed" : "Active"
            };

            return (true, Constant.CarePlanMessages.CarePlanCreated, response);
        }
        catch (Exception)
        {
            return (false, Constant.CarePlanMessages.SomethingWentWrong, null);
        }
    }

    public async Task<CarePlanDetailsDto> GetByIdAsync(int carePlanId)
    {
        if (carePlanId <= 0)
            throw new MediRecordsException("Invalid CarePlanId.");

        var carePlan = await _carePlanRepository.GetByIdAsync(carePlanId);

        if (carePlan == null)
            throw new MediRecordsException("Care plan not found.");

        return new CarePlanDetailsDto
        {
            CarePlanId = carePlan.CarePlanId,
            PatientId = carePlan.PatientId,
            PatientName = carePlan.PatientIdNavigation!.Name,
            GoalsJSON = carePlan.GoalsJSON,
            Instructions = carePlan.Instructions,
            Status = carePlan.Status
        };
    }

    public async Task<IEnumerable<CarePlanDetailsDto>> GetCarePlansAsync(int? patientId, string? patientName, bool? status)
    {
        if (patientId.HasValue && patientId <= 0)
            throw new MediRecordsException("Invalid PatientId.");

        var plans = await _carePlanRepository.GetAsync(patientId, patientName, status);

        return plans.Select(cp => new CarePlanDetailsDto
        {
            CarePlanId = cp.CarePlanId,
            PatientId = cp.PatientId,
            PatientName = cp.PatientIdNavigation!.Name,
            GoalsJSON = cp.GoalsJSON,
            Instructions = cp.Instructions,
            Status = cp.Status
        });
    }
}