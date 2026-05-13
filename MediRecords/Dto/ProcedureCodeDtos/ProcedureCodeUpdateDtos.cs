using System;

namespace MediRecords.Dto.ProcedureCodeDtos;

public class ProcedureCodeUpdateDtos
{
    public required string Description {get; set;}
    public required decimal Price {get; set;}
}
