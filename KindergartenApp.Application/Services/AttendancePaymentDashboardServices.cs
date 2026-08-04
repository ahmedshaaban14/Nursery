using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Application.Interfaces;
using KindergartenApp.Core.Entities;
using KindergartenApp.Core.Interfaces;
using KindergartenApp.Infrastructure.Repositories;

namespace KindergartenApp.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IChildRepository _childRepository;

    public AttendanceService(IAttendanceRepository attendanceRepository, IChildRepository childRepository)
    {
        _attendanceRepository = attendanceRepository;
        _childRepository = childRepository;
    }

    public async Task<IEnumerable<AttendanceDto>> GetTodayAttendanceAsync()
    {
        var today = DateTime.Today;
        var children = await _childRepository.GetActiveChildrenAsync();

        var results = new List<AttendanceDto>();
        foreach (var child in children.OrderBy(c => c.FullName))
        {
            var existing = await _attendanceRepository.GetByChildAndDateAsync(child.Id, today);
            results.Add(existing == null
                ? new AttendanceDto
                {
                    ChildId = child.Id,
                    ChildName = child.FullName,
                    Date = today,
                    IsPresent = false
                }
                : MapToDto(existing, child.FullName));
        }

        return results;
    }

    public async Task<IEnumerable<AttendanceDto>> GetChildAttendanceAsync(int childId)
    {
        var child = await _childRepository.GetByIdAsync(childId);
        var toDate = DateTime.Today;
        var fromDate = toDate.AddYears(-1);

        var items = await _attendanceRepository.GetChildAttendanceAsync(childId, fromDate, toDate);
        return items.Select(a => MapToDto(a, child.FullName));
    }

    public async Task<AttendanceDto> MarkAttendanceAsync(int childId, DateTime date, bool isPresent)
    {
        var child = await _childRepository.GetByIdAsync(childId);
        var existing = await _attendanceRepository.GetByChildAndDateAsync(childId, date);

        if (existing == null)
        {
            var entity = new Attendance
            {
                ChildId = childId,
                Date = date.Date,
                IsPresent = isPresent
            };

            await _attendanceRepository.AddAsync(entity);
            return MapToDto(entity, child.FullName);
        }

        existing.IsPresent = isPresent;
        existing.Date = date.Date;
        await _attendanceRepository.UpdateAsync(existing);
        return MapToDto(existing, child.FullName);
    }

    public async Task<AttendanceDto?> GetAttendanceAsync(int childId, DateTime date)
    {
        var existing = await _attendanceRepository.GetByChildAndDateAsync(childId, date);
        if (existing == null)
            return null;

        var child = await _childRepository.GetByIdAsync(childId);
        return MapToDto(existing, child.FullName);
    }

    public async Task BulkMarkAttendanceAsync(Dictionary<int, bool> attendanceData, DateTime date)
    {
        foreach (var (childId, isPresent) in attendanceData)
        {
            await MarkAttendanceAsync(childId, date, isPresent);
        }
    }

    public async Task MarkAllPresentAsync(DateTime date)
    {
        var children = await _childRepository.GetActiveChildrenAsync();
        foreach (var child in children)
        {
            await MarkAttendanceAsync(child.Id, date, true);
        }
    }

    public async Task MarkAllAbsentAsync(DateTime date)
    {
        var children = await _childRepository.GetActiveChildrenAsync();
        foreach (var child in children)
        {
            await MarkAttendanceAsync(child.Id, date, false);
        }
    }

    public async Task<IEnumerable<AttendanceDto>> GetAttendanceByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        var attendances = await _attendanceRepository.GetAllAsync();
        var filtered = attendances.Where(a => a.Date >= fromDate && a.Date <= toDate);
        var results = new List<AttendanceDto>();
        
        foreach (var attendance in filtered)
        {
            var child = await _childRepository.GetByIdAsync(attendance.ChildId);
            results.Add(MapToDto(attendance, child?.FullName ?? string.Empty));
        }
        
        return results;
    }

    public async Task<IEnumerable<AttendanceDto>> GetAttendanceByClassroomAsync(int classroomId, DateTime date)
    {
        var children = await _childRepository.GetAllAsync();
        var classroomChildren = children.Where(c => c.ClassroomId == classroomId);
        var results = new List<AttendanceDto>();
        
        foreach (var child in classroomChildren)
        {
            var attendance = await _attendanceRepository.GetByChildAndDateAsync(child.Id, date);
            results.Add(attendance == null
                ? new AttendanceDto
                {
                    ChildId = child.Id,
                    ChildName = child.FullName,
                    Date = date,
                    IsPresent = false
                }
                : MapToDto(attendance, child.FullName));
        }
        
        return results;
    }

    public async Task<decimal> GetAttendancePercentageAsync(int childId, DateTime fromDate, DateTime toDate)
    {
        var attendances = await _attendanceRepository.GetChildAttendanceAsync(childId, fromDate, toDate);
        if (!attendances.Any())
            return 0;

        return (decimal)attendances.Count(a => a.IsPresent) / attendances.Count() * 100;
    }

    public async Task<Dictionary<int, decimal>> GetDailyAttendanceStatsAsync(DateTime fromDate, DateTime toDate)
    {
        var attendances = await _attendanceRepository.GetAllAsync();
        var filtered = attendances.Where(a => a.Date >= fromDate && a.Date <= toDate);
        var children = await _childRepository.GetActiveChildrenAsync();
        var totalChildren = children.Count();
        
        var stats = new Dictionary<int, decimal>();
        var currentDate = fromDate;
        
        while (currentDate <= toDate)
        {
            var dayAttendances = filtered.Where(a => a.Date.Date == currentDate.Date);
            var presentCount = dayAttendances.Count(a => a.IsPresent);
            var percentage = totalChildren > 0 ? (decimal)presentCount / totalChildren * 100 : 0;
            stats[currentDate.Day] = percentage;
            currentDate = currentDate.AddDays(1);
        }
        
        return stats;
    }

    private static AttendanceDto MapToDto(Attendance entity, string childName) => new()
    {
        Id = entity.Id,
        ChildId = entity.ChildId,
        ChildName = childName,
        Date = entity.Date,
        IsPresent = entity.IsPresent,
        Notes = entity.Notes
    };
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IChildRepository _childRepository;

    public PaymentService(IPaymentRepository paymentRepository, IChildRepository childRepository)
    {
        _paymentRepository = paymentRepository;
        _childRepository = childRepository;
    }

    public async Task<IEnumerable<PaymentDto>> GetChildPaymentsAsync(int childId)
    {
        var payments = await _paymentRepository.GetChildPaymentsAsync(childId);
        return payments.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetUnpaidPaymentsAsync()
    {
        var payments = await _paymentRepository.GetUnpaidPaymentsAsync();
        return payments.Select(MapToDto);
    }

    public async Task<PaymentDto> CreatePaymentAsync(PaymentDto dto)
    {
        var child = await _childRepository.GetByIdAsync(dto.ChildId);
        var date = dto.Date == default ? DateTime.Today : dto.Date;

        var entity = new Payment
        {
            ChildId = dto.ChildId,
            Amount = dto.Amount,
            Date = date,
            Month = date.Month,
            Year = date.Year,
            IsPaid = dto.IsPaid,
            Notes = dto.Notes
        };

        await _paymentRepository.AddAsync(entity);
        entity.Child = child;
        return MapToDto(entity);
    }

    public async Task<PaymentDto> MarkPaymentAsync(int paymentId, bool isPaid)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        payment.IsPaid = isPaid;
        await _paymentRepository.UpdateAsync(payment);
        return MapToDto(payment);
    }

    public Task<decimal> GetMonthlyRevenueAsync(int month, int year)
        => _paymentRepository.GetMonthlyRevenueAsync(month, year);

    public async Task<PaymentDto> CreatePaymentWithInstallmentsAsync(PaymentDto dto, List<PaymentInstallmentDto> installments)
    {
        var child = await _childRepository.GetByIdAsync(dto.ChildId);
        var date = dto.Date == default ? DateTime.Today : dto.Date;

        var entity = new Payment
        {
            ChildId = dto.ChildId,
            Amount = dto.Amount,
            Date = date,
            Month = date.Month,
            Year = date.Year,
            IsPaid = dto.IsPaid,
            Notes = dto.Notes,
            DueDate = dto.DueDate,
            LateFee = dto.LateFee,
            Discount = dto.Discount
        };

        await _paymentRepository.AddAsync(entity);
        entity.Child = child;
        
        // Create installments if provided
        if (installments != null && installments.Any())
        {
            foreach (var installmentDto in installments)
            {
                var installment = new PaymentInstallment
                {
                    PaymentId = entity.Id,
                    InstallmentNumber = installmentDto.InstallmentNumber,
                    Amount = installmentDto.Amount,
                    DueDate = installmentDto.DueDate,
                    Status = installmentDto.Status,
                    LateFee = installmentDto.LateFee,
                    Discount = installmentDto.Discount,
                    Notes = installmentDto.Notes
                };
                // Would need to add to repository and save
            }
        }

        return MapToDto(entity);
    }

    public async Task<PaymentDto> RecordPartialPaymentAsync(int paymentId, decimal amount)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        // Cannot directly set PaidAmount as it's computed from installments
        // This would need to work with installments instead
        // For now, mark as paid if full amount is reached
        payment.IsPaid = payment.Amount <= amount;
        payment.UpdatedAt = DateTime.UtcNow;
        
        await _paymentRepository.UpdateAsync(payment);
        return MapToDto(payment);
    }

    public async Task<PaymentDto> ApplyLateFeeAsync(int paymentId, decimal lateFee)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        payment.LateFee = lateFee;
        payment.UpdatedAt = DateTime.UtcNow;
        
        await _paymentRepository.UpdateAsync(payment);
        return MapToDto(payment);
    }

    public async Task<PaymentDto> ApplyDiscountAsync(int paymentId, decimal discount)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        payment.Discount = discount;
        payment.UpdatedAt = DateTime.UtcNow;
        
        await _paymentRepository.UpdateAsync(payment);
        return MapToDto(payment);
    }

    public async Task<IEnumerable<PaymentDto>> GetOverduePaymentsAsync()
    {
        var payments = await _paymentRepository.GetUnpaidPaymentsAsync();
        var overdue = payments.Where(p => p.DueDate < DateTime.Today && !p.IsPaid);
        return overdue.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentHistoryAsync(int childId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var payments = await _paymentRepository.GetChildPaymentsAsync(childId);
        
        if (fromDate.HasValue)
            payments = payments.Where(p => p.Date >= fromDate.Value);
        
        if (toDate.HasValue)
            payments = payments.Where(p => p.Date <= toDate.Value);
        
        return payments.Select(MapToDto);
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime fromDate, DateTime toDate)
    {
        var payments = await _paymentRepository.GetAllAsync();
        var filtered = payments.Where(p => p.Date >= fromDate && p.Date <= toDate && p.IsPaid);
        return filtered.Sum(p => p.Amount);
    }

    public async Task<decimal> GetOutstandingBalanceAsync(int childId)
    {
        var payments = await _paymentRepository.GetChildPaymentsAsync(childId);
        return payments.Where(p => !p.IsPaid).Sum(p => p.TotalAmount);
    }

    public async Task<string> GenerateReceiptAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        var child = await _childRepository.GetByIdAsync(payment.ChildId);
        
        // Generate receipt text - in production this would create a PDF
        return $"RECEIPT #{payment.Id}\n" +
               $"Date: {DateTime.Now:yyyy-MM-dd HH:mm}\n" +
               $"Child: {child?.FullName}\n" +
               $"Amount: ${payment.Amount:F2}\n" +
               $"Paid: ${payment.PaidAmount:F2}\n" +
               $"Status: {(payment.IsPaid ? "PAID" : "PARTIAL")}";
    }

    public async Task<List<PaymentDto>> GenerateMonthlyInvoiceAsync(int month, int year)
    {
        var payments = await _paymentRepository.GetAllAsync();
        var filtered = payments.Where(p => p.Month == month && p.Year == year && !p.IsPaid);
        return filtered.Select(MapToDto).ToList();
    }

    private static PaymentDto MapToDto(Payment entity) => new()
    {
        Id = entity.Id,
        ChildId = entity.ChildId,
        ChildName = entity.Child?.FullName ?? string.Empty,
        Amount = entity.Amount,
        Date = entity.Date,
        Month = entity.Month,
        Year = entity.Year,
        IsPaid = entity.IsPaid,
        Notes = entity.Notes
    };
}

public class DashboardService : IDashboardService
{
    private readonly IChildRepository _childRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public DashboardService(IChildRepository childRepository, IAttendanceRepository attendanceRepository, IPaymentRepository paymentRepository)
    {
        _childRepository = childRepository;
        _attendanceRepository = attendanceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<DashboardDto> GetDashboardDataAsync()
    {
        var today = DateTime.Today;
        var activeChildren = (await _childRepository.GetActiveChildrenAsync()).ToList();
        var todayAttendances = await _attendanceRepository.FindAsync(a => a.Date.Date == today && a.IsPresent);
        var unpaidPayments = (await _paymentRepository.GetUnpaidPaymentsAsync()).ToList();
        var monthlyIncome = await _paymentRepository.GetMonthlyRevenueAsync(today.Month, today.Year);
        var recentPaid = await _paymentRepository.GetRecentPaymentsAsync(10);

        return new DashboardDto
        {
            TotalChildren = activeChildren.Count,
            TodayAttendance = todayAttendances.Count(),
            UnpaidPaymentsCount = unpaidPayments.Count,
            MonthlyIncome = monthlyIncome,
            RecentPayments = recentPaid.Select(p => new RecentPaymentDto
            {
                ChildName = p.Child?.FullName ?? string.Empty,
                Amount = p.Amount,
                Date = p.Date
            }).ToList()
        };
    }
}

