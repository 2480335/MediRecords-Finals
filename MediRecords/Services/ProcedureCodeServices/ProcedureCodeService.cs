using System;
using System.Diagnostics;
using MediRecords.Domain.Entities;
using MediRecords.Dto.ProcedureCodeDtos;
using MediRecords.Repository.ProcedureCodeRepository;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace MediRecords.Services.ProcedureCodeServices;

public class ProcedureCodeService : IProcedureCodeService
{
    private readonly IProcedureCodeRepository _repo;

    public ProcedureCodeService(IProcedureCodeRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// This is for passing the data to repo layer to save the data to db
    /// </summary>
    /// <param name="dto">this is used to take input </param>
    /// <exception cref="BadHttpRequestException">if any field is missing or wrong then this exception will be thrown</exception>
    public async Task AddAsync(ProcedureCodeRequestDto dto)
    {
        if(dto.Code == 0)
        {
            throw new BadHttpRequestException(Utility.Constant.InvalidProcedureCode);
        }
        if(dto.Description == null)
        {
            throw new BadHttpRequestException(Utility.Constant.Description);
        }
        if(dto.Price <= 0)
        {
            throw new BadHttpRequestException(Utility.Constant.InvalidPrice);
        }
        var code = new ProcedureCode
        {
            Code = dto.Code.ToString(),
            Description = dto.Description,
            Price = dto.Price
        };
        await _repo.AddAsync(code);
    }

    /// <summary>
    /// this funtion will get all procedure code with the help of repo layer
    /// </summary>
    /// <returns>this will return the procedure code int the form of PocedureViewDtos</returns>
    public async Task<IEnumerable<ProcedureCodeViewDtos>> GetAllProcedure(string? filterCode)
    {
        var procedures = await _repo.GetAllProcedure(filterCode);

        return procedures.Select(p => new ProcedureCodeViewDtos
        {
            CodeId = p.CodeId,
            Code = p.Code ?? string.Empty,
            Description = p.Description ?? string.Empty,
            Price = p.Price
        });

    }

    /// <summary>
    /// this is used the pass the data to repo layer the update the procedure code
    /// </summary>
    /// <param name="CodeId">Procedure code with this id will be updated</param>
    /// <param name="dto">details that will be updated</param>
    /// <exception cref="BadHttpRequestException">if there is any field missing or wrong then this exception will be thrown</exception>
    public async Task UpdateProcedureAsync(int CodeId,ProcedureCodeUpdateDtos dto)
    {
        if(CodeId <= 0)
        {
            throw new BadHttpRequestException(Utility.Constant.InvalidProcedureCode);
        }
        if(dto.Description == null)
        {
            throw new BadHttpRequestException(Utility.Constant.Description);
        }
        if(dto.Price <= 0)
        {
            throw new BadHttpRequestException(Utility.Constant.InvalidPrice);
        }
        await _repo.UpdateProcedureAsync(CodeId,dto);
    }
}
