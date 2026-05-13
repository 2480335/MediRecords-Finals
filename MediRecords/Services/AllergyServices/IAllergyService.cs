using System;
using MediRecords.Dto.AllergyDtos;

namespace MediRecords.Services.AllergyServices;

public interface IAllergyService
{
    public Task CreateAllergyAsync(int patientId, AllergyCreateRequestDto dto, int userId);
}
