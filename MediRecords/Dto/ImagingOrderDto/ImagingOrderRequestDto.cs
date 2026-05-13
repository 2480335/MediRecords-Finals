using System.ComponentModel.DataAnnotations;
using MediRecords.Domain.Enums;

namespace MediRecords.Dto.ImagingOrderDto;

public class ImagingOrderRequestDto
{
    [Required]
    [EnumDataType(typeof(ImagingOrderStudyType))]
    public ImagingOrderStudyType? StudyType { get; set; }

    [Required]
    public string? Notes { get; set; }
}