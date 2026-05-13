using System;
using MediRecords.Domain.Entities;
using MediRecords.Dto.ImagingOrderDto;

namespace MediRecords.Repository.ImagingOrderRepository;

public interface IImagingOrderRepository
{
    Task AddAsync(ImagingOrder order);

    Task<List<ImagingOrder>> GetAllAsync(ImagingOrderFilterDto filter);
    
}
