using System;
using MediRecords.Domain.Entities;
using MediRecords.Utility;
using Microsoft.EntityFrameworkCore;

namespace MediRecords.Repository.ImagingReportRepository;

public class ImagingReportRepository : IImagingReportRepository
{
    private readonly MediRecordsDbContext _context;

    private readonly IWebHostEnvironment _webHostEnvironment;

    public ImagingReportRepository(MediRecordsDbContext context,IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// this is used to save file in wwwroot/imaging_roport folder
    /// </summary>
    /// <param name="file">this will recieve file</param>
    /// <returns></returns>
    public async Task<string> SaveFileAsync(IFormFile file)
    {
        string rootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    
        string uploadsFolder = Path.Combine(rootPath, "imaging_reports");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }
        string uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }
        return Path.Combine("imaging_reports", uniqueFileName);
        
    }

    /// <summary>
    /// this is used to insert the data in ImagingReport table
    /// </summary>
    /// <param name="report">obj of imaging report to save in db</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">if Imaging order is not found of given imaging order id</exception>
    /// <exception cref="InvalidOperationException">this will thow error if there is already report for given imaging_order id</exception>
    public async Task AddAsync(ImagingReport report)
    {
        var orderExists = await _context.ImagingOrders
            .AnyAsync(o => o.ImagingOrderId == report.ImagingOrderId);
        if (!orderExists)
        {
            throw new KeyNotFoundException(Constant.OrderNotFound);
        }
        var reportExists = await _context.ImagingReports
            .AnyAsync(r => r.ImagingOrderId == report.ImagingOrderId);
        if (reportExists){ 
            throw new InvalidOperationException(Constant.ReportAlreadyExists);
        }
        await _context.ImagingReports.AddAsync(report);
        await _context.SaveChangesAsync();
    }
}