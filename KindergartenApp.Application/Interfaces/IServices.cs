using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KindergartenApp.Application.DTOs;
using KindergartenApp.Core.Entities;

namespace KindergartenApp.Application.Interfaces;

public interface IAuthenticationService
{
    Task<bool> AuthenticateAsync(string username, string password);
    Task<bool> InitializeAdminAsync();
}

public interface IChildService
{
    Task<IEnumerable<ChildDto>> GetAllChildrenAsync();
    Task<ChildDto?> GetChildAsync(int id);
    Task<IEnumerable<ChildDto>> SearchChildrenAsync(string query);
    Task<ChildDto> CreateChildAsync(ChildDto dto);
    Task<ChildDto> UpdateChildAsync(ChildDto dto);
    Task DeleteChildAsync(int id);
}

public interface IAttendanceService
{
    Task<IEnumerable<AttendanceDto>> GetTodayAttendanceAsync();
    Task<IEnumerable<AttendanceDto>> GetChildAttendanceAsync(int childId);
    Task<AttendanceDto> MarkAttendanceAsync(int childId, DateTime date, bool isPresent);
    Task<AttendanceDto?> GetAttendanceAsync(int childId, DateTime date);
    Task BulkMarkAttendanceAsync(Dictionary<int, bool> attendanceData, DateTime date);
    Task MarkAllPresentAsync(DateTime date);
    Task MarkAllAbsentAsync(DateTime date);
    Task<IEnumerable<AttendanceDto>> GetAttendanceByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<IEnumerable<AttendanceDto>> GetAttendanceByClassroomAsync(int classroomId, DateTime date);
    Task<decimal> GetAttendancePercentageAsync(int childId, DateTime fromDate, DateTime toDate);
    Task<Dictionary<int, decimal>> GetDailyAttendanceStatsAsync(DateTime fromDate, DateTime toDate);
}

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetChildPaymentsAsync(int childId);
    Task<IEnumerable<PaymentDto>> GetUnpaidPaymentsAsync();
    Task<PaymentDto> CreatePaymentAsync(PaymentDto dto);
    Task<PaymentDto> MarkPaymentAsync(int paymentId, bool isPaid);
    Task<decimal> GetMonthlyRevenueAsync(int month, int year);
    Task<PaymentDto> CreatePaymentWithInstallmentsAsync(PaymentDto dto, List<PaymentInstallmentDto> installments);
    Task<PaymentDto> RecordPartialPaymentAsync(int paymentId, decimal amount);
    Task<PaymentDto> ApplyLateFeeAsync(int paymentId, decimal lateFee);
    Task<PaymentDto> ApplyDiscountAsync(int paymentId, decimal discount);
    Task<IEnumerable<PaymentDto>> GetOverduePaymentsAsync();
    Task<IEnumerable<PaymentDto>> GetPaymentHistoryAsync(int childId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<decimal> GetTotalRevenueAsync(DateTime fromDate, DateTime toDate);
    Task<decimal> GetOutstandingBalanceAsync(int childId);
    Task<string> GenerateReceiptAsync(int paymentId);
    Task<List<PaymentDto>> GenerateMonthlyInvoiceAsync(int month, int year);
}

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardDataAsync();
}

public interface IReportService
{
    Task<byte[]> GenerateUnpaidChildrenReportAsync();
    Task<byte[]> GenerateMonthlyRevenueReportAsync(int month, int year);
    Task<byte[]> GenerateAttendanceReportAsync(int childId, DateTime fromDate, DateTime toDate);
}

public interface IBackupService
{
    Task BackupDatabaseAsync();
    string GetBackupFolderPath();
}


// New service interfaces for additional modules

public interface IClassroomService
{
    Task<IEnumerable<ClassroomDto>> GetAllClassroomsAsync();
    Task<ClassroomDto?> GetClassroomAsync(int id);
    Task<ClassroomDto> CreateClassroomAsync(ClassroomDto dto);
    Task<ClassroomDto> UpdateClassroomAsync(ClassroomDto dto);
    Task DeleteClassroomAsync(int id);
    Task<IEnumerable<ClassroomDto>> GetClassroomsByStageAsync(ClassroomStage stage);
    Task AssignTeacherAsync(int classroomId, int teacherId);
    Task AssignChildToClassroomAsync(int childId, int classroomId);
}

public interface IStaffService
{
    Task<IEnumerable<StaffDto>> GetAllStaffAsync();
    Task<StaffDto?> GetStaffAsync(int id);
    Task<IEnumerable<StaffDto>> GetStaffByRoleAsync(StaffRole role);
    Task<StaffDto> CreateStaffAsync(StaffDto dto);
    Task<StaffDto> UpdateStaffAsync(StaffDto dto);
    Task DeleteStaffAsync(int id);
    Task<IEnumerable<StaffDto>> GetActiveStaffAsync();
    Task MarkStaffAttendanceAsync(int staffId, DateTime date, bool isPresent);
}

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserAsync(int id);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<UserDto> CreateUserAsync(UserDto dto, string password);
    Task<UserDto> UpdateUserAsync(UserDto dto);
    Task DeleteUserAsync(int id);
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task UpdateLastLoginAsync(int userId);
}

public interface IAlertService
{
    Task<IEnumerable<AlertDto>> GetAllAlertsAsync();
    Task<IEnumerable<AlertDto>> GetActiveAlertsAsync();
    Task<IEnumerable<AlertDto>> GetAlertsByTypeAsync(AlertType type);
    Task<IEnumerable<AlertDto>> GetAlertsForChildAsync(int childId);
    Task<AlertDto> CreateAlertAsync(AlertDto dto);
    Task<AlertDto> UpdateAlertAsync(AlertDto dto);
    Task DeleteAlertAsync(int id);
    Task MarkAlertAsReadAsync(int id);
    Task MarkAlertAsResolvedAsync(int id);
    Task CheckAndGenerateAlertsAsync();
}

public interface IDocumentService
{
    Task<IEnumerable<DocumentDto>> GetDocumentsByChildAsync(int childId);
    Task<DocumentDto?> GetDocumentAsync(int id);
    Task<DocumentDto> UploadDocumentAsync(DocumentDto dto, byte[] fileData);
    Task DeleteDocumentAsync(int id);
    Task<byte[]?> DownloadDocumentAsync(int id);
}

public interface ISearchService
{
    Task<SearchDto> GlobalSearchAsync(string query);
}

public interface IAnalyticsService
{
    Task<DashboardDto> GetEnhancedDashboardDataAsync();
    Task<List<ClassroomOccupancyDto>> GetClassroomOccupancyAsync();
    Task<decimal> GetAttendanceTrendAsync(int days);
    Task<decimal> GetRevenueTrendAsync(int months);
}