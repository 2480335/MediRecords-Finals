using System;
using MediRecords.Domain.Enums;

namespace MediRecords.Dto.ProcedureCodeDtos;

public class ProcedureCodeRequestDto
{
    public required Procedure Code {get; set;}

    public string Description {get; set;} = string.Empty;

    public decimal Price {get; set;}

}
