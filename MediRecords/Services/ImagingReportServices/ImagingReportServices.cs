using System;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using MediRecords.Domain.Entities;
using MediRecords.Dto.ImagingReportDto;
using MediRecords.Repository.ImagingReportRepository;
using MediRecords.Utility;

namespace MediRecords.Services.ImagingReportServices;

public class ImagingReportServices : IImagingReportServices
{
    private readonly IImagingReportRepository _repo;

    public ImagingReportServices(IImagingReportRepository repo)
    {
        _repo = repo;
    }

    
    /// <summary>
    /// this is used to validate the file type only pdf and images are allowed to upload
    /// </summary>
    /// <param name="file"></param>
    /// <exception cref="ArgumentException">this will throw exception if file type is wrong</exception>
    private void ValidateReportFile(IFormFile file)
    {
        var allowedMimeTypes = new[]
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };

        var allowedExtensions = new[]
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png"
        };

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException(Utility.Constant.AllowedFile);

        if (!allowedMimeTypes.Contains(file.ContentType))
            throw new ArgumentException(Utility.Constant.AllowedFile);
    }

    /// <summary>
    /// this is used to pass the data in Report format to the repo layer
    /// </summary>
    /// <param name="ImagingOrderID">this refers to the id for imaging order for which report is geting inserted</param>
    /// <param name="dto">this will take file and pass the the repo layer</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">this will throw error if file is empty</exception>
    public async Task CreateReportAsync(int ImagingOrderID, ImagingReportRequestDto dto)
    {
        if (dto == null)
            throw new ArgumentException(Constant.RequestCannotBeNull);

        if (dto.Findings == null)
            throw new ArgumentException(Constant.InvalidFindings);

        string? savedFilePath = null;

        if (dto.ReportFile != null)
        {
            if (dto.ReportFile.Length == 0)
                throw new ArgumentException(Utility.Constant.FileLength);

            ValidateReportFile(dto.ReportFile);

            savedFilePath = await _repo.SaveFileAsync(dto.ReportFile);
        }

        var report = new ImagingReport
        {
            ImagingOrderId = ImagingOrderID,
            Findings = JsonSerializer.Serialize(dto.Findings),
            Impression = dto.Impression,
            AttachmentPath = savedFilePath,
            ReportDate = DateTime.UtcNow,
            Status = true
        };

        await _repo.AddAsync(report);
    }
}