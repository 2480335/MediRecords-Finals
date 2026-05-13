using System;
using MediRecords.Dto.ProblemListDtos;

namespace MediRecords.Services.ProblemListServices;

public interface IProblemListService
{
    Task CreateProblemAsync(int patientId, ProblemCreateRequestDto dto, int userId);
}
