using System;

namespace MediRecords.Dto.ProcedureCodeDtos;

public class ProcedureCodeViewDtos
{
    public required int CodeId {get; set;}

    public required string Code {get; set;}

    public required string Description {get; set;}

    public required decimal Price {get; set;}

}
