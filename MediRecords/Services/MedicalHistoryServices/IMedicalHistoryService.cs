using System;
using MediRecords.Dto.MedicalHistoryDtos;

namespace MediRecords.Services.MedicalHistoryServices;

public interface IMedicalHistoryService
{  
    public Task CreateMedicalHistoryAsync(int patientId, MedicalHistoryCreateRequestDto dto, int userId);
}  
