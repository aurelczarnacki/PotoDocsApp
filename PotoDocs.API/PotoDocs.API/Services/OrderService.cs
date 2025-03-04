﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PotoDocs.API.Entities;
using PotoDocs.API.Models;
using PotoDocs.Shared.Models;
using System.IO.Compression;
using System.Net;

namespace PotoDocs.API.Services;

public interface IOrderService
{
    ApiResponse<IEnumerable<OrderDto>> GetAll(int page = 1, int pageSize = 5, string? driverEmail = null);
    ApiResponse<OrderDto> GetById(int id);
    void Delete(int invoiceNumber);
    void Update(int invoiceNumber, OrderDto dto);
    Task<ApiResponse<OrderDto>> ProcessAndCreateOrderFromPdf(IFormFile file);
    Task<ApiResponse<OrderDto>> AddCMRFileAsync(List<IFormFile> files, int invoiceNumber);
    void DeleteCMR(string fileName);
    Task<byte[]> CreateInvoicePDF(int invoiceNumber);
    Task<byte[]> CreateInvoices(int year, int month);
    Task<Dictionary<int, List<int>>> GetAvailableYearsAndMonthsAsync();
}

public class OrderService : IOrderService
{
    private readonly PotodocsDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IOpenAIService _openAIService;
    private readonly IInvoiceService _invoiceService;

    public OrderService(PotodocsDbContext dbContext, IMapper mapper, IOpenAIService openAIService, IInvoiceService invoiceService)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _openAIService = openAIService;
        _invoiceService = invoiceService;
    }

    public ApiResponse<IEnumerable<OrderDto>> GetAll(int page = 1, int pageSize = 5, string? driverEmail = null)
    {
        var query = _dbContext.Orders.Include(o => o.Driver)
                                     .Include(o => o.CMRFiles)
                                     .AsQueryable();

        if (!string.IsNullOrEmpty(driverEmail))
        {
            query = query.Where(o => o.Driver != null && o.Driver.Email == driverEmail);
        }

        var totalItems = query.Count();

        var orders = query.Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToList();

        var ordersDto = _mapper.Map<List<OrderDto>>(orders);

        return ApiResponse<IEnumerable<OrderDto>>.Success(ordersDto);
    }


    public ApiResponse<OrderDto> GetById(int id)
    {
        var order = _dbContext.Orders.Include(o => o.Driver)
                                     .Include(c => c.CMRFiles)
                                     .FirstOrDefault(o => o.InvoiceNumber == id);
        if (order == null) return ApiResponse<OrderDto>.Failure("Nie znaleziono zlecenia.", HttpStatusCode.BadRequest);

        return ApiResponse<OrderDto>.Success(_mapper.Map<OrderDto>(order));
    }

    public void Delete(int invoiceNumber)
    {
        var order = _dbContext.Orders.FirstOrDefault(o => o.InvoiceNumber == invoiceNumber);
        if (order == null) return;

        _dbContext.Orders.Remove(order);
        _dbContext.SaveChanges();
    }

    public void Update(int invoiceNumber, OrderDto dto)
    {
        var order = _dbContext.Orders
            .Include(o => o.Driver)
            .Include(o => o.CMRFiles)
            .FirstOrDefault(o => o.InvoiceNumber == invoiceNumber);

        if (order == null) return;

        _mapper.Map(dto, order);

        if (dto.Driver != null)
        {
            order.Driver = _dbContext.Users.FirstOrDefault(u => u.Email == dto.Driver.Email);
        }

        _dbContext.SaveChanges();
    }

    public async Task<ApiResponse<OrderDto>> ProcessAndCreateOrderFromPdf(IFormFile file)
    {
        if (file == null || file.Length == 0 || file.ContentType != "application/pdf")
        {
            return ApiResponse<OrderDto>.Failure("Plik jest nieprawidłowy lub ma niepoprawny format", HttpStatusCode.BadRequest);
        }

        var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs");
        if (!Directory.Exists(uploadsFolderPath))
        {
            Directory.CreateDirectory(uploadsFolderPath);
        }

        var fileName = $"{Guid.NewGuid()}.pdf";
        var filePath = Path.Combine(uploadsFolderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var extractedData = await _openAIService.GetInfoFromText(file);
        extractedData.InvoiceIssueDate = extractedData.UnloadingDate ?? DateTime.Now;
        extractedData.InvoiceNumber = GetInvoiceNumber(extractedData.UnloadingDate ?? DateTime.Now);
        extractedData.PDFUrl = fileName;

        var order = _mapper.Map<Order>(extractedData);
        _dbContext.Orders.Add(order);
        _dbContext.SaveChanges();

        return ApiResponse<OrderDto>.Success(extractedData);
    }

    private int GetInvoiceNumber(DateTime date)
    {
        int invoiceNumber = _dbContext.Orders.Where(o => o.InvoiceIssueDate.Value.Month == date.Month
                                                      && o.InvoiceIssueDate.Value.Year == date.Year).Count() + 1;
        return int.Parse($"{invoiceNumber:D2}{date.Month:D2}{date.Year}");
    }

    public async Task<ApiResponse<OrderDto>> AddCMRFileAsync(List<IFormFile> files, int invoiceNumber)
    {
        if (files == null || files.Count == 0) return ApiResponse<OrderDto>.Failure("Nie przesłano pliku", HttpStatusCode.BadRequest);
        var order = _dbContext.Orders.FirstOrDefault(o => o.InvoiceNumber == invoiceNumber);
        if (order == null) return ApiResponse<OrderDto>.Failure("Nie znaleziono zlecenia.", HttpStatusCode.BadRequest);

        var cmrFileUrls = new List<string>();
        foreach (var file in files)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/pdfs", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = Path.Combine("/pdfs", fileName);
            cmrFileUrls.Add(relativePath);

            var cmrFile = new CMRFile
            {
                Url = fileName,
                OrderId = order.Id,
                Order = order
            };
            _dbContext.CMRFiles.Add(cmrFile);
            _dbContext.SaveChanges();
        }
        return GetById(invoiceNumber);

    }

    public void DeleteCMR(string fileName)
    {
        var cmrFile = _dbContext.CMRFiles.FirstOrDefault(c => c.Url == fileName);
        if (cmrFile == null) return;

        _dbContext.CMRFiles.Remove(cmrFile);
        _dbContext.SaveChanges();

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cmrFile.Url.TrimStart('/'));

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
    public async Task<byte[]> CreateInvoicePDF(int invoiceNumber)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.InvoiceNumber == invoiceNumber);
        if (order == null) return null;

        return await _invoiceService.GenerateInvoicePdf(order);
    }
    public async Task<byte[]> CreateInvoices(int year, int month)
    {
        var orders = await _dbContext.Orders
            .Where(o => o.InvoiceIssueDate.Value.Month == month && o.InvoiceIssueDate.Value.Year == year)
            .ToListAsync();

        if (orders == null || orders.Count == 0) return null;

        string tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);

        try
        {
            foreach (var order in orders)
            {
                var pdfData = await _invoiceService.GenerateInvoicePdf(order);

                string fileName = $"FAKTURA_{FormatInvoiceNumber((int)order.InvoiceNumber)}.pdf";
                string filePath = Path.Combine(tempDirectory, fileName);

                await File.WriteAllBytesAsync(filePath, pdfData);
            }

            string zipFilePath = Path.Combine(Path.GetTempPath(), $"Faktury_{month:D2}-{year}.zip");

            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            ZipFile.CreateFromDirectory(tempDirectory, zipFilePath);

            return await File.ReadAllBytesAsync(zipFilePath);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }
    }
    private string FormatInvoiceNumber(int invoiceNumber)
    {
        string invoiceNumberStr = invoiceNumber.ToString("D7");

        string numberPart = invoiceNumberStr.Substring(0, invoiceNumberStr.Length - 6);
        string monthPart = invoiceNumberStr.Substring(invoiceNumberStr.Length - 6, 2);
        string yearPart = invoiceNumberStr.Substring(invoiceNumberStr.Length - 4, 4);

        return $"FAKTURA {numberPart}-{monthPart}-{yearPart}";
    }
    public async Task<Dictionary<int, List<int>>> GetAvailableYearsAndMonthsAsync()
    {
        var data = await _dbContext.Orders
            .Where(o => o.InvoiceIssueDate.HasValue)
            .Select(o => new { o.InvoiceIssueDate!.Value.Year, o.InvoiceIssueDate!.Value.Month })
            .ToListAsync();

        return data
            .GroupBy(o => o.Year)
            .OrderByDescending(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Select(o => o.Month).Distinct().OrderBy(m => m).ToList()
            );
    }
}
