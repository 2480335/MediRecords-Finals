using System;
using AutoMapper;
using MediRecords.Domain.Entities;
using MediRecords.Dto.EncounterDtos.Response;

namespace MediRecords.MappingProfiles;

public class EncounterMappingProfile : Profile
{
    public EncounterMappingProfile()
    {
        // Encounter -> EncounterSummaryDto
        CreateMap<Encounter, EncounterSummaryDto>()
            .ForMember(dest => dest.PatientName,
                opt => opt.MapFrom(src => src.PatientIdNavigation != null
                    ? src.PatientIdNavigation.Name : "Unknown"))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

        // Encounter -> EncounterDetailDto
        CreateMap<Encounter, EncounterDetailDto>()
            .ForMember(dest => dest.PatientName,
                opt => opt.MapFrom(src => src.PatientIdNavigation != null
                    ? src.PatientIdNavigation.Name : "Unknown"))
            .ForMember(dest => dest.ProviderName,
                opt => opt.MapFrom(src => src.ProviderIdNavigation != null
                    ? src.ProviderIdNavigation.Name : "Unknown"))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

        // Encounter -> EncounterLookupDto
        CreateMap<Encounter, EncounterLookupDto>()
            .ForMember(dest => dest.PatientName,
                opt => opt.MapFrom(src => src.PatientIdNavigation != null
                    ? src.PatientIdNavigation.Name : "Unknown"))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
