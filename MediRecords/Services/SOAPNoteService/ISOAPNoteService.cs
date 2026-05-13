using System;
using MediRecords.Dto.SOAPNoteDtos.Request;
using MediRecords.Dto.SOAPNoteDtos.Response;

namespace MediRecords.Services.SOAPNoteService;

public interface ISOAPNoteService
{
    Task<(bool Success, String Message, SOAPNoteResponseDto? Data)> SaveSOAPNoteAsync(int encounterId, SaveSOAPNoteRequestDto dto);
}
