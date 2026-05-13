using System;
using MediRecords.Domain.Entities;

namespace MediRecords.Dto.LabOrderDtos;


public class LabOrderResponseDto
{
    public int LabOrderId { get; set; }
    public int EncounterId { get; set; }
    public int OrderedBy { get; set; }
    public string TestJson { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public bool Status { get; set; }

    public static LabOrderResponseDto FromEntity(LabOrder entity)
    {
        return new LabOrderResponseDto
        {
            LabOrderId = entity.LabOrderId,
            EncounterId = entity.EncounterId,
            OrderedBy = entity.OrderedBy,
            TestJson = entity.TestJson,
            OrderDate = entity.OrderDate,
            Status = entity.Status
        };
    }
}


