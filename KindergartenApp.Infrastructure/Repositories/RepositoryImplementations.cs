using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query;
using System.Globalization;

using KindergartenApp.Core.Entities;
using KindergartenApp.Infrastructure.Data;


namespace KindergartenApp.Infrastructure.Repositories;

public class ChildRepository : Repository<Child>, IChildRepository
{
    public ChildRepository(KindergartenDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Child>> SearchAsync(string query)
    {
        return await _dbSet
            .Where(c => c.FullName.Contains(query) || c.ParentName.Contains(query) || c.Phone.Contains(query))
            .ToListAsync();
    }

    public async Task<IEnumerable<Child>> GetActiveChildrenAsync()
    {
        return await _dbSet.Where(c => c.IsActive).ToListAsync();
    }
}

public class AttendanceRepository : Repository<Attendance>, IAttendanceRepository
{
    public AttendanceRepository(KindergartenDbContext context) : base(context)
    {
    }

    public async Task<Attendance?> GetByChildAndDateAsync(int childId, DateTime date)
    {
        return await _dbSet.FirstOrDefaultAsync(a => a.ChildId == childId && a.Date.Date == date.Date);
    }

    public async Task<IEnumerable<Attendance>> GetChildAttendanceAsync(int childId, DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .Where(a => a.ChildId == childId && a.Date >= fromDate && a.Date <= toDate)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
    }
}

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(KindergartenDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Payment>> GetChildPaymentsAsync(int childId)
    {
        return await _dbSet
            .Where(p => p.ChildId == childId)
            .Include(p => p.Child)
            .OrderByDescending(p => p.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetUnpaidPaymentsAsync()
    {
        return await _dbSet.Where(p => !p.IsPaid).Include(p => p.Child).ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetRecentPaymentsAsync(int count)
    {
        return await _dbSet
            .Where(p => p.IsPaid)
            .Include(p => p.Child)
            .OrderByDescending(p => p.Date)
            .Take(count)
            .ToListAsync();
    }

    public async Task<decimal> GetMonthlyRevenueAsync(int month, int year)
    {
        // SQLite doesn't support aggregating over decimal consistently (often stored as TEXT).
        // Materialize then sum on client side for correctness.
        var amounts = await _dbSet
            .Where(p => p.Month == month && p.Year == year && p.IsPaid)
            .Select(p => p.Amount)
            .ToListAsync();
        return amounts.Sum();
    }
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(KindergartenDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }
}

// New repository implementations for additional modules

public class ClassroomRepository : Repository<Classroom>, IClassroomRepository
{
    public ClassroomRepository(KindergartenDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Classroom>> GetClassroomsByStageAsync(ClassroomStage stage)
    {
        return await _dbSet.Where(c => c.Stage == stage).ToListAsync();
    }

    public async Task<Classroom?> GetClassroomWithChildrenAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Children)
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}

public class StaffRepository : Repository<Staff>, IStaffRepository
{
    public StaffRepository(KindergartenDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Staff>> GetStaffByRoleAsync(StaffRole role)
    {
        return await _dbSet.Where(s => s.Role == role).ToListAsync();
    }

    public async Task<IEnumerable<Staff>> GetActiveStaffAsync()
    {
        return await _dbSet.Where(s => s.Status == StaffStatus.Active).ToListAsync();
    }
}

public class AlertRepository : Repository<Alert>, IAlertRepository
{
    public AlertRepository(KindergartenDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Alert>> GetActiveAlertsAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(a => !a.IsResolved && !a.IsRead && (a.ExpiryDate == null || a.ExpiryDate > now))
            .OrderByDescending(a => a.Priority)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alert>> GetAlertsByTypeAsync(AlertType type)
    {
        return await _dbSet.Where(a => a.Type == type).OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Alert>> GetAlertsForChildAsync(int childId)
    {
        return await _dbSet.Where(a => a.ChildId == childId).OrderByDescending(a => a.CreatedAt).ToListAsync();
    }
}

public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(KindergartenDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Document>> GetDocumentsByChildAsync(int childId)
    {
        return await _dbSet.Where(d => d.ChildId == childId).OrderByDescending(d => d.CreatedAt).ToListAsync();
    }
}

public class PaymentInstallmentRepository : Repository<PaymentInstallment>, IPaymentInstallmentRepository
{
    public PaymentInstallmentRepository(KindergartenDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PaymentInstallment>> GetInstallmentsByPaymentAsync(int paymentId)
    {
        return await _dbSet.Where(i => i.PaymentId == paymentId).OrderBy(i => i.InstallmentNumber).ToListAsync();
    }
}