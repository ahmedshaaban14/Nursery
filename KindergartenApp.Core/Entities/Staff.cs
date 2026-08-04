using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KindergartenApp.Core.Entities;

public class Staff : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public StaffRole Role { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Salary { get; set; }

    [Required]
    [MaxLength(20)]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Address { get; set; }

    [Required]
    public DateTime HireDate { get; set; }

    [Required]
    public StaffStatus Status { get; set; } = StaffStatus.Active;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
    public ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();

    [NotMapped]
    public string StatusDisplay => Status.ToString();
    
    [NotMapped]
    public string RoleDisplay => Role.ToString();
    
    [NotMapped]
    public int YearsOfService => (int)((DateTime.Now - HireDate).TotalDays / 365);
}
