using System;
using MediRecords.Dto.BillingDtos.Request;
using MediRecords.Dto.BillingDtos.Response;
using AutoMapper;
using MediRecords.Domain.Entities;
using MediRecords.Repository.BillingRepo;
using MediRecords.Utility;
using System.Text;
using System.Text.Json;


namespace MediRecords.Services.BillingServices;

public class BillingService : IBillingService
{
    private readonly IBillingRepository _billingRepository;
    private readonly IMapper _mapper;

    private static readonly List<string> ValidFormats  = new() { "csv", "json" };
    private static readonly List<string> ValidStatuses = new() { "unbilled", "billed", "all" };

    public BillingService(IBillingRepository billingRepository, IMapper mapper)
    {
        _billingRepository = billingRepository;
        _mapper = mapper;
    }

    public async Task<(bool Success, string Message, VisitChargeResponseDto? Data, int StatusCode)> AssignVisitChargeAsync(AssignVisitChargeRequestDto dto)
    {
        try
        {
            if(dto.EncounterId <= 0)
                return (false, Constant.BillingMessages.InvalidEncounterId, null, 400);

            if(dto.CodeId <= 0)
                return (false, Constant.BillingMessages.InvalidCodeId, null, 400);
            
            var encounterExists = await _billingRepository.EncounterExistsAsync(dto.EncounterId);
            if(!encounterExists)
                return (false, Constant.BillingMessages.EncounterNotFound, null, 404);
            
            var procedureCode = await _billingRepository.GetProcedureCodeByIdAsync(dto.CodeId);
            if(procedureCode == null)
                return (false, Constant.BillingMessages.ProcedureCodeNotFound, null, 404);

            var chargeExists = await _billingRepository.ChargeExistsAsync(dto.EncounterId, dto.CodeId);
            if(chargeExists)
                return (false, Constant.BillingMessages.DuplicateCharge, null, 409);

            var amount = dto.Amount.HasValue && dto.Amount.Value > 0 ? dto.Amount.Value : procedureCode.Price;

            var charge = new VisitChargeRef
            {
                EncounterId = dto.EncounterId,
                CodeId = dto.CodeId,
                Amount = amount,
                Status = false
            };

            var saved = await _billingRepository.AddAsync(charge);
            var response = _mapper.Map<VisitChargeResponseDto>(saved);

            return (true, Constant.BillingMessages.ChargeCreated, response, 201);
        }
        catch (Exception)
        {
            return (false, Constant.BillingMessages.SomethingWentWrong, null, 500);
        }
    }
    public async Task<(bool Success, string Message, VisitChargeResponseDto? Data, int StatusCode)>
        UpdateVisitChargeAmountAsync(int chargeId, UpdateVisitChargeRequestDto dto)
    {
        try
        {
            // Validate ChargeId
            if (chargeId <= 0)
                return (false, Constant.BillingMessages.InvalidChargeId, null, 400);

            // Validate Amount
            if (dto.Amount <= 0)
                return (false, Constant.BillingMessages.InvalidAmount, null, 400);

            // Check charge exists
            var charge = await _billingRepository.GetChargeByIdAsync(chargeId);
            if (charge == null)
                return (false, Constant.BillingMessages.ChargeNotFound, null, 404);

            if (charge.Status) // true = Billed
                return (false, Constant.BillingMessages.ChargeAlreadyBilled, null, 400);
            // Update amount
            charge.Amount = dto.Amount;

            var updated  = await _billingRepository.UpdateAsync(charge);
            var response = _mapper.Map<VisitChargeResponseDto>(updated);

            return (true, Constant.BillingMessages.ChargeUpdated, response, 200);
        }
        catch (Exception)
        {
            return (false, Constant.BillingMessages.SomethingWentWrong, null, 500);
        }
    }
    public async Task<(bool Success, string Message, IEnumerable<VisitChargeResponseDto>? Data, int StatusCode)>
        GetChargesByEncounterIdAsync(int encounterId)
    {
        try
        {
            if (encounterId <= 0)
                return (false, Constant.BillingMessages.InvalidEncounterId, null, 400);

            var encounterExists = await _billingRepository.EncounterExistsAsync(encounterId);
            if (!encounterExists)
                return (false, Constant.BillingMessages.EncounterNotFound, null, 404);

            var charges  = await _billingRepository.GetChargesByEncounterIdAsync(encounterId);
            var response = _mapper.Map<IEnumerable<VisitChargeResponseDto>>(charges);

            return (true, string.Empty, response, 200);
        }
        catch (Exception)
        {
            return (false, Constant.BillingMessages.SomethingWentWrong, null, 500);
        }
    }
    public async Task<(bool Success, string Message, PagedResponseDto<UnbilledEncounterResponseDto>? Data, int StatusCode)>
        GetUnbilledEncountersAsync(DateTime? fromDate, DateTime? toDate, int? providerId, int page, int pageSize)
    {
        try
        {
            if (page <= 0)
                return (false, Constant.BillingMessages.InvalidPageNumber, null, 400);

            if (pageSize <= 0 || pageSize > 100)
                return (false, Constant.BillingMessages.InvalidPageSize, null, 400);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
                return (false, Constant.BillingMessages.InvalidDateRange, null, 400);

            var (encounters, totalCount) = await _billingRepository
                .GetUnbilledEncountersAsync(fromDate, toDate, providerId, page, pageSize);

            var data = encounters.Select(e => new UnbilledEncounterResponseDto
            {
                EncounterId = e.EncounterId,
                PatientId = e.PatientId,
                PatientName = e.PatientIdNavigation?.Name ?? "Unknown",
                ProviderId = e.ProviderId,
                ProviderName = e.ProviderIdNavigation?.Name ?? "Unknown",
                VisitType = e.VisitType,
                Date = e.Date,
                TotalChargeAmount = e.VisitChargeRefs
                                       .Where(v => !v.Status)
                                       .Sum(v => v.Amount),
                UnbilledChargeCount = e.VisitChargeRefs
                                       .Count(v => !v.Status)
            });

            var response = new PagedResponseDto<UnbilledEncounterResponseDto>
            {
                Data = data,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return (true, string.Empty, response, 200);
        }
        catch (Exception)
        {
            return (false, Constant.BillingMessages.SomethingWentWrong, null, 500);
        }
    }
    public async Task<(bool Success, string Message, MarkBilledResponseDto? Data, int StatusCode)>
        MarkChargesAsBilledAsync(MarkChargesBilledRequestDto dto, int userId)
    {
        try
        {
            if (dto.ChargeIds == null || dto.ChargeIds.Count == 0)
                return (false, Constant.BillingMessages.ChargeIdsRequired, null, 400);

            var distinctIds = dto.ChargeIds.Distinct().ToList();

            var charges = await _billingRepository.GetChargesByIdsAsync(distinctIds);

            var foundIds      = charges.Select(c => c.ChargeId).ToList();
            var notFoundIds   = distinctIds.Except(foundIds).ToList();
            var alreadyBilled = charges.Where(c => c.Status).Select(c => c.ChargeId).ToList();
            var toBill        = charges.Where(c => !c.Status).ToList(); // false = Unbilled

            toBill.ForEach(c => c.Status = true);

            if (toBill.Any())
            {
                await _billingRepository.UpdateRangeAsync(toBill);

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = "MarkAsBilled",
                    Resource = $"VisitChargeRefs: [{string.Join(",", toBill.Select(c => c.ChargeId))}]",
                    TimeStamp = DateTime.UtcNow
                };

                await _billingRepository.AddAuditLogAsync(auditLog);
            }

            var response = new MarkBilledResponseDto
            {
                MarkedAsBilled = toBill.Select(c => c.ChargeId).ToList(),
                AlreadyBilled  = alreadyBilled,
                NotFound = notFoundIds,
                Message = Constant.BillingMessages.ChargesMarkedBilled
            };

            return (true, Constant.BillingMessages.ChargesMarkedBilled, response, 200);
        }
        catch (Exception)
        {
            return (false, Constant.BillingMessages.SomethingWentWrong, null, 500);
        }
    }

    public async Task<(bool Success, string Message, byte[]? FileContent, string ContentType, string FileName, int StatusCode)>
        ExportChargesAsync(string format, string status, DateTime? fromDate, DateTime? toDate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(format) || !ValidFormats.Contains(format.ToLower()))
                return (false, Constant.BillingMessages.UnsupportedFormat, null, string.Empty, string.Empty, 400);

            if (string.IsNullOrWhiteSpace(status) || !ValidStatuses.Contains(status.ToLower()))
                return (false, Constant.BillingMessages.UnsupportedStatus, null, string.Empty, string.Empty, 400);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
                return (false, Constant.BillingMessages.InvalidDateRange, null, string.Empty, string.Empty, 400);

            var charges = await _billingRepository.GetChargesForExportAsync(status, fromDate, toDate);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            if (format.ToLower() == "csv")
            {
                var csv = BuildCsv(charges);
                var bytes = Encoding.UTF8.GetBytes(csv);
                var fileName = $"visit-charges-{status}-{timestamp}.csv";
                return (true, string.Empty, bytes, "text/csv", fileName, 200);
            }
            else
            {
                var exportData = charges.Select(v => new
                {
                    v.ChargeId,
                    v.EncounterId,
                    PatientName = v.EncounterIdNavigation?.PatientIdNavigation?.Name ?? "Unknown",
                    Code = v.ProcedureCodeNavigation?.Code ?? string.Empty,
                    Description = v.ProcedureCodeNavigation?.Description ?? string.Empty,
                    v.Amount,
                    Status = v.Status ? "Billed" : "Unbilled",
                    EncounterDate = v.EncounterIdNavigation?.Date
                });

                var json = JsonSerializer.Serialize(exportData,
                    new JsonSerializerOptions { WriteIndented = true });
                var bytes = Encoding.UTF8.GetBytes(json);
                var fileName = $"visit-charges-{status}-{timestamp}.json";
                return (true, string.Empty, bytes, "application/json", fileName, 200);
            }
        }
        catch (Exception)
        {
            return (false, Constant.BillingMessages.SomethingWentWrong, null, string.Empty, string.Empty, 500);
        }
    }

    private static string BuildCsv(IEnumerable<VisitChargeRef> charges)
    {
        var sb = new StringBuilder();

        sb.AppendLine("ChargeId,EncounterId,PatientName,Code,Description,Amount,Status,EncounterDate");

        foreach (var v in charges)
        {
            var patientName = v.EncounterIdNavigation?.PatientIdNavigation?.Name ?? "Unknown";
            var code = v.ProcedureCodeNavigation?.Code ?? string.Empty;
            var description = v.ProcedureCodeNavigation?.Description   ?? string.Empty;
            var status = v.Status ? "Billed" : "Unbilled";
            var encounterDate = v.EncounterIdNavigation?.Date.ToString("yyyy-MM-dd") ?? string.Empty;

            sb.AppendLine(
                $"{v.ChargeId},{v.EncounterId},\"{patientName}\",{code},\"{description}\",{v.Amount},{status},{encounterDate}");
        }

        return sb.ToString();
    }
}