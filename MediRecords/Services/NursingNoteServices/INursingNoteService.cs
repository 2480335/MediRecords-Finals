using MediRecords.Dto.NursingNoteDtos;

namespace MediRecords.Services.NursingNoteServices;

public interface INursingNoteService
{
    Task<NursingNoteResponseDto> AddNoteAsync(int encounterId, NursingNoteCreateRequestDto requestDto,string recordedBy);
    Task<NursingNoteResponseDto> UpdateNoteAsync(int noteId, NursingNoteUpdateRequestDto requestDto,string recordedBy);
}
