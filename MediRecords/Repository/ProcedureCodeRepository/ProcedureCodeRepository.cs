using System;
using System.Reflection.Metadata;
using MediRecords.Domain.Entities;
using MediRecords.Dto.ProcedureCodeDtos;
using MediRecords.Dto.UserDtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.ProcedureCodeRepository;

public class ProcedureCodeRepository : IProcedureCodeRepository
{

    private readonly MediRecordsDbContext _context;
    public ProcedureCodeRepository(MediRecordsDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// This function is for Adding the ProcedureCode to the db
    /// </summary>
    /// <exception cref="InvalidOperationException">This will throw error if we try to insert the the duplicate data</exception>
    public async Task AddAsync(ProcedureCode code)
    {
        var existing = await _context.ProcedureCodes.FirstOrDefaultAsync(p => p.Code == code.Code);
        if (existing != null)
        {
            throw new InvalidOperationException(Utility.Constant.ProcedureFound);
        }
        await _context.ProcedureCodes.AddAsync(code);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// This is for getting all the procedure code
    /// </summary>
    /// <returns>this will return all the procedure code</returns>
    public async Task<IEnumerable<ProcedureCode>> GetAllProcedure(string? filterCode)
    {
        var query = _context.ProcedureCodes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(filterCode))
        {
            query = query.Where(p => (p.Code ?? string.Empty).Contains(filterCode));
        }
        return await query.ToListAsync();
    }

    /// <summary>
    /// This is for updating the Procdure Code, Only two fields are allowed to get updated
    /// </summary>
    /// <param name="id">this id for procedure code that we want to update</param>
    /// <param name="dto">data that has to be updated</param>
    /// <exception cref="BadHttpRequestException">if there is no procedure for given id then this exception will be thrown</exception>
    public async Task UpdateProcedureAsync(int id, ProcedureCodeUpdateDtos dto)
    {
        var procedure = await _context.ProcedureCodes.FirstOrDefaultAsync(p => p.CodeId == id);

        if (procedure == null)
        {
            throw new KeyNotFoundException(Utility.Constant.ProcedureNotFound);
        }

        procedure.Description = dto.Description;
        procedure.Price = dto.Price;

        await _context.SaveChangesAsync();
    }

}
