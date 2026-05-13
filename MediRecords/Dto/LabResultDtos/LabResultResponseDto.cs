using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Dto.LabResultDtos;

public class LabResultResponseDto
{
    public int ResultId { get; set; }
    public int LabOrderId { get; set; }
    public string? ResultJson { get; set; }
    public DateTime ResultDate { get; set; }
    public bool Status { get; set; }

    public static LabResultResponseDto FromEntity(LabResult entity)
    {
        return new LabResultResponseDto
        {
            ResultId = entity.ResultId,
            LabOrderId = entity.LabOrderId,
            ResultJson = entity.ResultJson,
            ResultDate = entity.ResultDate,
            Status = entity.Status
        };
    }
}
