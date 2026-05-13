using System;
using System.Diagnostics;
using System.Text.Json;
using MediRecords.Domain.Entities;
using MediRecords.Dto.SOAPNoteDtos.Request;
using MediRecords.Dto.SOAPNoteDtos.Response;

namespace MediRecords.Utility;

public static class SOAPNoteHelper
{
    // ── Serialize DTO → Entity
    public static string SerializeSubjective(string hpi, string? ros)
    {
        return JsonSerializer.Serialize(new { HPI = hpi, ROS = ros });
    }
    
    public static string? SerializeObjective(string? examFindings, string? observations)
    {
        if(string.IsNullOrWhiteSpace(examFindings) && string.IsNullOrWhiteSpace(observations))
        {
            return null;
        }

        return JsonSerializer.Serialize(new
        {
           ExamFindings = examFindings,
           Observations = observations 
        });
    }

    // ── Deserialize Entity → Response DTO 
    public static SOAPNoteResponseDto ToResponseDto(SOAPNote note)
    {
        var dto = new SOAPNoteResponseDto
        {
            NoteId = note.NoteId,
            EncounterId = note.EncounterId,
            Assessment = note.Assessment,
            Plan = note.Plan,
            Status = note.Status ? "Draft" : "Signed & Locked",
            CreatedDate = note.CreatedDate
        };

        if (!string.IsNullOrWhiteSpace(note.Subjective))
        {
            try
            {   
                var subj = JsonSerializer.Deserialize<SubjectiveJson>(note.Subjective);
                dto.HPI = subj?.HPI ?? string.Empty;
                dto.ROS = subj?.ROS;
            }
            catch
            {
                dto.HPI = note.Subjective;
            }
        }

        if (!string.IsNullOrWhiteSpace(note.Objective))
        {
            try
            {
                var obj = JsonSerializer.Deserialize<ObjectiveJson>(note.Objective);
                dto.ExamFindings = obj?.ExamFindings;
                dto.Observations = obj?.Observations;
            }
            catch
            {
                dto.ExamFindings = note.Objective;
            }
        }

        return dto;
    }

    private class SubjectiveJson
    {
        public string? HPI { get; set; }
        public string? ROS { get; set; }
    }

    private class ObjectiveJson
    {
        public string? ExamFindings { get; set; }
        public string? Observations { get; set; }
    }
}