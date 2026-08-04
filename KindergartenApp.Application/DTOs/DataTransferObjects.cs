using System;
using System.Collections.Generic;
using KindergartenApp.Core.Entities;

namespace KindergartenApp.Application.DTOs;

public class ChildDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal MonthlyFee { get; set; }
    public DateTime JoinDate { get; set; }
    public bool IsActive { get; set; }
    
    // New fields
    public int? ClassroomId { get; set; }
    public string? ClassroomName { get; set; }
    public string? MedicalNotes { get; set; }
    public string? Allergies { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? PhotoPath { get; set; }
    
    // Computed properties
    public decimal OutstandingBalance { get; set; }
    public int TotalPayments { get; set; }
}

public class AttendanceDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public bool IsPresent { get; set; }
    public string? Notes { get; set; }
    
    // New fields
    public int? ClassroomId { get; set; }
    public string? ClassroomName { get; set; }
    public int? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
}

public class PaymentDto
{
    public int Id { get; set; }
    public int ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public bool IsPaid { get; set; }
    public string? Notes { get; set; }
    
    // New fields
    public bool SupportsInstallments { get; set; }
    public decimal LateFee { get; set; }
    public decimal Discount { get; set; }
    public string? ReceiptNumber { get; set; }
    public DateTime? DueDate { get; set; }
    public PaymentStatus Status { get; set; }
    
    // Computed properties
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingBalance { get; set; }
    public bool IsOverdue { get; set; }
}

public class DashboardDto
{
    public int TotalChildren { get; set; }
    public int TodayAttendance { get; set; }
    public int UnpaidPaymentsCount { get; set; }
    public decimal MonthlyIncome { get; set; }
    public List<RecentPaymentDto> RecentPayments { get; set; } = new();
    
    // New analytics
    public int TotalClassrooms { get; set; }
    public int TotalStaff { get; set; }
    public int ActiveAlerts { get; set; }
    public decimal AttendancePercentage { get; set; }
    public List<ClassroomOccupancyDto> ClassroomOccupancy { get; set; } = new();
}

public class RecentPaymentDto
{
    public string ChildName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

// New DTOs for additional modules

public class ClassroomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ClassroomStage Stage { get; set; }
    public int Capacity { get; set; }
    public int? TeacherId { get; set; }
    public string? TeacherName { get; set; }
    public string? Notes { get; set; }
    
    // Computed properties
    public int CurrentStudentCount { get; set; }
    public bool IsFull { get; set; }
    public double OccupancyPercentage { get; set; }
}

public class StaffDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public StaffRole Role { get; set; }
    public decimal Salary { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime HireDate { get; set; }
    public StaffStatus Status { get; set; }
    public string? Notes { get; set; }
    
    // Computed properties
    public int YearsOfService { get; set; }
    public int AssignedClassroomsCount { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int? StaffId { get; set; }
    public string? StaffName { get; set; }
}

public class AlertDto
{
    public int Id { get; set; }
    public AlertType Type { get; set; }
    public AlertPriority Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public bool IsRead { get; set; }
    public bool IsResolved { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? ChildId { get; set; }
    public string? ChildName { get; set; }
    public int? StaffId { get; set; }
    public string? StaffName { get; set; }
    
    // Computed properties
    public bool IsExpired { get; set; }
}

public class DocumentDto
{
    public int Id { get; set; }
    public DocumentType Type { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public int? ChildId { get; set; }
    public string? ChildName { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Computed properties
    public string FileSizeDisplay { get; set; }
}

public class PaymentInstallmentDto
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public int InstallmentNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public PaymentStatus Status { get; set; }
    public decimal LateFee { get; set; }
    public decimal Discount { get; set; }
    public string? Notes { get; set; }
    
    // Computed properties
    public decimal TotalAmount { get; set; }
    public bool IsOverdue { get; set; }
}

public class ClassroomOccupancyDto
{
    public string ClassroomName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentCount { get; set; }
    public double OccupancyPercentage { get; set; }
}

public class SearchDto
{
    public string Query { get; set; } = string.Empty;
    public List<ChildSearchResultDto> Children { get; set; } = new();
    public List<StaffSearchResultDto> Staff { get; set; } = new();
    public List<ClassroomSearchResultDto> Classrooms { get; set; } = new();
}

public class ChildSearchResultDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? ClassroomName { get; set; }
}

public class StaffSearchResultDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public StaffRole Role { get; set; }
    public string Phone { get; set; } = string.Empty;
    public StaffStatus Status { get; set; }
}

public class ClassroomSearchResultDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ClassroomStage Stage { get; set; }
    public string? TeacherName { get; set; }
    public int Capacity { get; set; }
    public int CurrentCount { get; set; }
}

public class ReportDto
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ReportDataDto> Data { get; set; } = new();
}

public class ReportDataDto
{
    public string Category { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

