using System;
using AutoMapper;
using MediRecords.Domain.Entities;
using MediRecords.Dto.BillingDtos.Response;

namespace MediRecords.MappingProfiles;

public class BillingMappingProfile : Profile
{
    public BillingMappingProfile()
    {
        CreateMap<VisitChargeRef, VisitChargeResponseDto>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.ProcedureCodeNavigation != null ? src.ProcedureCodeNavigation.Code : string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.ProcedureCodeNavigation != null ? src.ProcedureCodeNavigation.Description : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status ? "Billed" : "Unbilled"));
    }
}