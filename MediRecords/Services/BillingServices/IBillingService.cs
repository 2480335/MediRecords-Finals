using System;
using MediRecords.Dto.BillingDtos.Request;
using MediRecords.Dto.BillingDtos.Response;
namespace MediRecords.Services.BillingServices;

public interface IBillingService
{
    Task<(bool Success, string Message, VisitChargeResponseDto? Data, int StatusCode)>
       AssignVisitChargeAsync(AssignVisitChargeRequestDto dto);

    Task<(bool Success, string Message, VisitChargeResponseDto? Data, int StatusCode)>
        UpdateVisitChargeAmountAsync(int chargeId, UpdateVisitChargeRequestDto dto);
    
    Task<(bool Success, string Message, IEnumerable<VisitChargeResponseDto>? Data, int StatusCode)>
        GetChargesByEncounterIdAsync(int encounterId);
    
    Task<(bool Success, string Message, PagedResponseDto<UnbilledEncounterResponseDto>? Data, int StatusCode)>
        GetUnbilledEncountersAsync(DateTime? fromDate, DateTime? toDate, int? providerId, int page, int pageSize);

    Task<(bool Success, string Message, MarkBilledResponseDto? Data, int StatusCode)>
        MarkChargesAsBilledAsync(MarkChargesBilledRequestDto dto, int userId);
    
    Task<(bool Success, string Message, byte[]? FileContent, string ContentType, string FileName, int StatusCode)>
        ExportChargesAsync(string format, string status, DateTime? fromDate, DateTime? toDate);
}
