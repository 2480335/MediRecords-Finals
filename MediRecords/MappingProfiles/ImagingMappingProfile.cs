using AutoMapper;
using MediRecords.Domain.Entities;
using MediRecords.Dto.ImagingDtos.Response;

namespace MediRecords.MappingProfiles;

public class ImagingMappingProfile : Profile
{
    public ImagingMappingProfile()
    {
        CreateMap<ImagingReport, ImagingReportResponseDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status ? "Active" : "Inactive"));
    }
}