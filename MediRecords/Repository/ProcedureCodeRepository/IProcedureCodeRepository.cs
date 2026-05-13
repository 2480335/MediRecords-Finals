using System;
using MediRecords.Domain.Entities;
using MediRecords.Dto.ProcedureCodeDtos;

namespace MediRecords.Repository.ProcedureCodeRepository;

public interface IProcedureCodeRepository
{
    Task AddAsync(ProcedureCode code);

    Task<IEnumerable<ProcedureCode>> GetAllProcedure(string? filterCode);

    Task UpdateProcedureAsync(int id,ProcedureCodeUpdateDtos dto);

}
