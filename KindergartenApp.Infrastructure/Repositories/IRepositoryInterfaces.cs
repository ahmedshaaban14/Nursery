using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KindergartenApp.Core.Entities;
using KindergartenApp.Core.Interfaces;


namespace KindergartenApp.Infrastructure.Repositories;

public interface IChildRepository : IRepository<Child>
{
    Task<IEnumerable<Child>> SearchAsync(string query);
    Task<IEnumerable<Child>> GetActiveChildrenAsync();
}

public interface IAttendanceRepository : IRepository<Attendance>
{
    Task<Attendance?> GetByChildAndDateAsync(int childId, DateTime date);
    Task<IEnumerable<Attendance>> GetChildAttendanceAsync(int childId, DateTime fromDate, DateTime toDate);
}

public interface IPaymentRepository : IRepository<Payment>
{
    Task<IEnumerable<Payment>> GetChildPaymentsAsync(int childId);
    Task<IEnumerable<Payment>> GetUnpaidPaymentsAsync();
    Task<IEnumerable<Payment>> GetRecentPaymentsAsync(int count);
    Task<decimal> GetMonthlyRevenueAsync(int month, int year);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
}

// New repository interfaces for additional modules

public interface IClassroomRepository : IRepository<Classroom>
{
    Task<IEnumerable<Classroom>> GetClassroomsByStageAsync(ClassroomStage stage);
    Task<Classroom?> GetClassroomWithChildrenAsync(int id);
}

public interface IStaffRepository : IRepository<Staff>
{
    Task<IEnumerable<Staff>> GetStaffByRoleAsync(StaffRole role);
    Task<IEnumerable<Staff>> GetActiveStaffAsync();
}

public interface IAlertRepository : IRepository<Alert>
{
    Task<IEnumerable<Alert>> GetActiveAlertsAsync();
    Task<IEnumerable<Alert>> GetAlertsByTypeAsync(AlertType type);
    Task<IEnumerable<Alert>> GetAlertsForChildAsync(int childId);
}

public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetDocumentsByChildAsync(int childId);
}

public interface IPaymentInstallmentRepository : IRepository<PaymentInstallment>
{
    Task<IEnumerable<PaymentInstallment>> GetInstallmentsByPaymentAsync(int paymentId);
}