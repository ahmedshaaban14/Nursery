using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.Infrastructure.Repositories;


namespace KindergartenApp.Application.Services;

public class BackupService : IBackupService
{
    private readonly string _databasePath;

    public BackupService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _databasePath = Path.Combine(appDataPath, "KindergartenApp", "kindergarten.db");
    }

    public async Task BackupDatabaseAsync()
    {
        var backupFolder = GetBackupFolderPath();
        if (!Directory.Exists(backupFolder))
            Directory.CreateDirectory(backupFolder);

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        var backupPath = Path.Combine(backupFolder, $"kindergarten_backup_{timestamp}.db");

        if (File.Exists(_databasePath))
        {
            await Task.Run(() => File.Copy(_databasePath, backupPath, true));
        }
    }

    public string GetBackupFolderPath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataPath, "KindergartenApp", "Backups");
    }
}

public class ReportService : IReportService
{
    private readonly IChildRepository _childRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IAttendanceRepository _attendanceRepository;

    public ReportService(IChildRepository childRepository, IPaymentRepository paymentRepository, IAttendanceRepository attendanceRepository)
    {
        _childRepository = childRepository;
        _paymentRepository = paymentRepository;
        _attendanceRepository = attendanceRepository;
    }

    public async Task<byte[]> GenerateUnpaidChildrenReportAsync()
    {
        var unpaidPayments = await _paymentRepository.GetUnpaidPaymentsAsync();
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Unpaid Payments");

        worksheet.Cell("A1").Value = "Child Name";
        worksheet.Cell("B1").Value = "Amount";
        worksheet.Cell("C1").Value = "Due Date";
        worksheet.Cell("D1").Value = "Month";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        foreach (var payment in unpaidPayments.OrderBy(p => p.Child!.FullName))
        {
            worksheet.Cell($"A{row}").Value = payment.Child?.FullName ?? "Unknown";
            worksheet.Cell($"B{row}").Value = payment.Amount;
            worksheet.Cell($"C{row}").Value = payment.Date.ToString("yyyy-MM-dd");
            worksheet.Cell($"D{row}").Value = $"{payment.Month}/{payment.Year}";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    public async Task<byte[]> GenerateMonthlyRevenueReportAsync(int month, int year)
    {
        var payments = await _paymentRepository.GetUnpaidPaymentsAsync();
        var monthlyPayments = payments.Where(p => p.Month == month && p.Year == year).ToList();

        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Monthly Revenue");

        worksheet.Cell("A1").Value = "Child Name";
        worksheet.Cell("B1").Value = "Amount";
        worksheet.Cell("C1").Value = "Payment Date";
        worksheet.Cell("D1").Value = "Status";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 2;
        decimal totalRevenue = 0;

        foreach (var payment in monthlyPayments.OrderBy(p => p.Child!.FullName))
        {
            worksheet.Cell($"A{row}").Value = payment.Child?.FullName ?? "Unknown";
            worksheet.Cell($"B{row}").Value = payment.Amount;
            worksheet.Cell($"C{row}").Value = payment.Date.ToString("yyyy-MM-dd");
            worksheet.Cell($"D{row}").Value = payment.IsPaid ? "Paid" : "Unpaid";
            if (payment.IsPaid)
                totalRevenue += payment.Amount;
            row++;
        }

        worksheet.Cell($"A{row + 1}").Value = "Total Revenue";
        worksheet.Cell($"B{row + 1}").Value = totalRevenue;
        worksheet.Row(row + 1).Style.Font.Bold = true;

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    public async Task<byte[]> GenerateAttendanceReportAsync(int childId, DateTime fromDate, DateTime toDate)
    {
        var child = await _childRepository.GetByIdAsync(childId);
        if (child == null)
            throw new InvalidOperationException("Child not found");

        var attendances = await _attendanceRepository.GetChildAttendanceAsync(childId, fromDate, toDate);

        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance");

        worksheet.Cell("A1").Value = $"Attendance Report - {child.FullName}";
        worksheet.Cell("A2").Value = $"Period: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}";

        worksheet.Cell("A4").Value = "Date";
        worksheet.Cell("B4").Value = "Status";

        var headerRow = worksheet.Row(4);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        int row = 5;
        int presentCount = 0;
        foreach (var attendance in attendances.OrderBy(a => a.Date))
        {
            worksheet.Cell($"A{row}").Value = attendance.Date.ToString("yyyy-MM-dd");
            worksheet.Cell($"B{row}").Value = attendance.IsPresent ? "Present" : "Absent";
            if (attendance.IsPresent)
                presentCount++;
            row++;
        }

        worksheet.Cell($"A{row + 1}").Value = "Summary";
        worksheet.Cell($"A{row + 2}").Value = "Total Days";
        worksheet.Cell($"B{row + 2}").Value = attendances.Count();
        worksheet.Cell($"A{row + 3}").Value = "Present Days";
        worksheet.Cell($"B{row + 3}").Value = presentCount;
        worksheet.Cell($"A{row + 4}").Value = "Absent Days";
        worksheet.Cell($"B{row + 4}").Value = attendances.Count() - presentCount;

        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }
}
