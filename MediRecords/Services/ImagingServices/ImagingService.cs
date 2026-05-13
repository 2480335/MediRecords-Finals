using MediRecords.Dto.ImagingDtos.Response;
using MediRecords.Repository.ImagingRepo;
using AutoMapper;
using MediRecords.Utility;

namespace MediRecords.Services.ImagingServices;

public class ImagingService : IImagingService
{
    private readonly IImagingRepository _imagingRepository;
    private readonly IMapper _mapper;

    public ImagingService(IImagingRepository imagingRepository, IMapper mapper)
    {
        _imagingRepository = imagingRepository;
        _mapper = mapper;
    }

    public async Task<(bool Success, string Message, IEnumerable<ImagingReportResponseDto>? Data)>
        GetReportsByOrderIdAsync(int imagingOrderId)
    {
        try
        {
            // Validate imaging order ID
            if (imagingOrderId <= 0)
                return (false, Constant.ImagingMessages.InvalidImagingOrderId, null);

            // Check if imaging order exists
            var orderExists = await _imagingRepository.OrderExistsAsync(imagingOrderId);
            if (!orderExists)
                return (false, Constant.ImagingMessages.ImagingOrderNotFound, null);

            // Get all reports for this order
            var reports = await _imagingRepository.GetReportsByOrderIdAsync(imagingOrderId);

            if (!reports.Any())
                return (false, Constant.ImagingMessages.NoReportsFound, null);

            var result = _mapper.Map<IEnumerable<ImagingReportResponseDto>>(reports);

            return (true, string.Empty, result);
        }
        catch (Exception)
        {
            return (false, Constant.ImagingMessages.SomethingWentWrong, null);
        }
    }
}