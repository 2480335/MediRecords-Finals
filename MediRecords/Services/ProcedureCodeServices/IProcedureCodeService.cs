using System;
using MediRecords.Dto.ProcedureCodeDtos;

namespace MediRecords.Services.ProcedureCodeServices;

public interface IProcedureCodeService
{
    Task AddAsync(ProcedureCodeRequestDto dto);

    Task<IEnumerable<ProcedureCodeViewDtos>> GetAllProcedure(string filterCode);

    Task UpdateProcedureAsync(int CodeId,ProcedureCodeUpdateDtos dto);
}
