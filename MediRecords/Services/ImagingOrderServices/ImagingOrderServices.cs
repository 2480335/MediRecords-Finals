using System;
using System.Text.Json;
using MediRecords.Domain.Entities;
using MediRecords.Dto.ImagingOrderDto;
using MediRecords.Repository.ImagingOrderRepository;
using MediRecords.Services.AuthServices;
using MediRecords.Utility;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace MediRecords.Services.ImagingOrderServices;

public class ImagingOrderServices : IImagingOrderServices
{
    private readonly IImagingOrderRepository _repo;
    private readonly IAuthService _authService;

    public ImagingOrderServices(IImagingOrderRepository repo, IAuthService authService)
    {
        _repo = repo;
        _authService = authService;
    }

    /// <summary>
    /// Processes an imaging order request by validating the encounter ID, 
    /// mapping the DTO to a domain entity, and persisting it via the repository.
    /// </summary>
    /// <param name="dto">The data transfer object containing imaging order details.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the EncounterID is less than or equal to zero.</exception>
    public async Task AddAsync(int EncounterID, ImagingOrderRequestDto dto, int userId)
    {
        if (EncounterID <= 0)
        {
            throw new ArgumentException(Constant.InvalidEncounterId);
        }
        if (dto.StudyType < 0)
        {
            throw new ArgumentException(Constant.StudyType);
        }
        if (string.IsNullOrWhiteSpace(dto.Notes))
        {
            throw new ArgumentException(Constant.Notes);
        }
        var order = new ImagingOrder
        {
            EncounterId = EncounterID,
            StudyType = dto.StudyType!.Value,
            Notes = dto.Notes,
            OrderedDate = DateTime.UtcNow,
            Status = true
        };
        await _repo.AddAsync(order);
        await _authService.SaveAuditLog(
            userId,
            $"Imaging Order created for Encounter Id :{EncounterID}"
        );
    }

    /// <summary>
    /// Gets imaging orders based on the given filter.
    /// </summary>
    /// <param name="filter">Filter criteria for retrieving imaging orders.  </param>
    /// <returns> A list of imaging orders with their related reports. </returns>
    /// <exception cref="KeyNotFoundException"> Thrown when no imaging orders match the provided filter.</exception>
    public async Task<List<ImagingOrderResponseDto>> GetAllAsync(ImagingOrderFilterDto filter)
    {
        var orders = await _repo.GetAllAsync(filter);

        if ((filter != null && filter.HasAnyValue()) && !orders.Any())
        {
            throw new KeyNotFoundException(Constant.ImagingOrderNotExists);
        }

        return orders.Select(o => new ImagingOrderResponseDto
        {
            ImagingOrderId = o.ImagingOrderId,
            EncounterId = o.EncounterId,
            StudyType = o.StudyType,
            Notes = o.Notes,
            OrderedDate = o.OrderedDate,
            Status = o.Status,
            Reports = o.ImagingReports.Select(r => new ImagingReportResponseDto
            {
                ReportId = r.ReportId,
                ImagingOrderId = r.ImagingOrderId,
                Impression = r.Impression,
                ReportDate = r.ReportDate,
                Status = r.Status,
                Findings = r.Findings
            }).ToList()
        }).ToList();
    }
}